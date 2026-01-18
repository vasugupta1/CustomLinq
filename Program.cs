using System.Collections;

var source = Enumerable.Range(0, 1000).ToArray();
Console.WriteLine(Enumerable.Select(source, i => i * 2).Sum());
Console.WriteLine(Select(source, i => i * 2).Sum());
Console.WriteLine(SelectManual(source, i => i * 2).Sum());


static IEnumerable<TResult> Select<TSource,TResult>(IEnumerable<TSource> source, Func<TSource,TResult> selector)
{
    ArgumentNullException.ThrowIfNull(source, nameof(source));
    ArgumentNullException.ThrowIfNull(selector, nameof(selector));


    // Remember untill enumrator.MoveNext() is called no part of the function actually runs hence why we do this
    static IEnumerable<TResult> Impl(IEnumerable<TSource> source, Func<TSource,TResult> selector)
    {
        foreach(var val in source)
        {
            yield return selector.Invoke(val);
        }
    }

    return Impl(source, selector);
}


static IEnumerable<TResult> SelectManual<TSource,TResult>(IEnumerable<TSource> source, Func<TSource,TResult> selector)
{
    ArgumentNullException.ThrowIfNull(source, nameof(source));
    ArgumentNullException.ThrowIfNull(selector, nameof(selector));

    return new SelectManualEnumerable<TSource, TResult>(source, selector);
}

sealed class SelectManualEnumerable<TSource, TResult> : IEnumerable<TResult>
{
    private IEnumerable<TSource> _source;
    private Func<TSource,TResult> _selector;

    public SelectManualEnumerable(IEnumerable<TSource> source, Func<TSource,TResult> selector)
    {
        _source = source;
        _selector = selector;
    }

    public IEnumerator<TResult> GetEnumerator() => new Enumrator<TSource, TResult>(_source, _selector);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

sealed class Enumrator<TSource, TResult> : IEnumerator<TResult>
{
    private IEnumerable<TSource> _source;
    private Func<TSource,TResult> _selector;
    private TResult _current = default!;
    private IEnumerator<TSource>? _enumrator;
    //Move next hasn't beencalledyet
    private EnumratorState _state = EnumratorState.Begining;

     public Enumrator(IEnumerable<TSource> source, Func<TSource,TResult> selector)
    {
        _source = source;
        _selector = selector;
    }

    public TResult Current => _current;

    object IEnumerator.Current => Current!;

    public void Dispose()
    {
        _state = EnumratorState.Ended;
        _enumrator?.Dispose();
    }

    public bool MoveNext()
    {
        if(_state is EnumratorState.Begining)
        {
            _enumrator = _source.GetEnumerator();
            _state = EnumratorState.Iterating;
        }

        if(_state is EnumratorState.Iterating)
        {
            try
            {
                if(_enumrator!.MoveNext())
                {
                    _current = _selector.Invoke(_enumrator.Current);
                    return true;
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }
        Dispose();
        return false;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }
}

public enum EnumratorState
{
    Begining,
    Iterating,
    Ended
}