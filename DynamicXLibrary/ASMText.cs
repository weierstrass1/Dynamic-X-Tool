namespace DynamicXLibrary
{
    public class ASMText
    {
        public string Code { get; private set; }
        public int Size { get; private set; }

        public ASMText(string code, int size)
        {
            Code = code;
            Size = size;
        }
        public override string ToString()
        {
            return Code;
        }
    }
}
