namespace NaturalDisastersRenewal.Common.Types
{
    public class Triplet<T1, T2, T3>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        internal Triplet(T1 first, T2 second, T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }
}
