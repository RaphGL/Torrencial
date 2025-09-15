namespace Torrencial;

class Torrencial
{
    static void Main(string[] args)
    {
        var bencode = new Bencode.Parser("./test.bencode");
        var value = bencode.Parse();
        Console.WriteLine(value);
    }
}
