namespace Torrencial.Bencode;

using System.Collections.Generic;

public class BEDictionary
{
    private SortedDictionary<string, object>  dict = new SortedDictionary<string, object>();

    public BEDictionary() { }
    public void Add(string key, Int64 value) => dict.Add(key, value);
    public void Add(string key, string value) => dict.Add(key, value);
    public void Add(string key, BEList value) => dict.Add(key, value);
    public void Add(string key, BEDictionary value) => dict.Add(key, value);

    // TODO: add more operations as needed
}

public class BEList
{
    private List<object> items = new List<object>();

    public BEList() { }

    public void Add(Int64 item) => items.Add(item);
    public void Add(string item) => items.Add(item);
    public void Add(BEDictionary item) => items.Add(item);
    public void Add(BEList item) => items.Add(item);

    // TODO: add more operations as needed
}

public class Parser
{
    private StreamReader fileStream;

    public Parser(string path)
    {
        fileStream = new StreamReader(path);
    }

    // TODO: find out what to do next to get useful information with bencode
    public string Parse()
    {
        var str = ParseByteString();
        var i64 = ParseInt();
        var list = ParseList();
        var dict = ParseDictionary();
        Console.WriteLine(i64);
        Console.WriteLine(list);
        Console.WriteLine(dict);
        return str;
    }

    private string ParseByteString()
    {
        if (!Char.IsDigit((char)fileStream.Peek()))
        {
            throw new InvalidDataException();
        }

        string strSizeStr = "";
        while (true)
        {
            var curr = fileStream.Peek();
            if (curr == ':')
            {
                fileStream.Read();
                break;
            }

            var numChar = (char)fileStream.Read();
            if (!Char.IsDigit(numChar))
            {
                throw new InvalidDataException();
            }
            strSizeStr += numChar;
        }

        int num = int.Parse(strSizeStr);
        string bytestr = "";
        foreach (var i in Enumerable.Range(0, num))
        {
            var c = (char)fileStream.Read();
            bytestr += c;
        }

        return bytestr;
    }

    private Int64 ParseInt()
    {
        if (fileStream.Peek() != 'i')
        {
            throw new InvalidDataException("expected value with format i<number>e");
        }
        fileStream.Read();

        var numStr = "";

        if (fileStream.Peek() == '-')
        {
            numStr += '-';
            fileStream.Read();
        }

        while (true)
        {
            var c = (char)fileStream.Peek();
            if (c == 'e')
            {
                fileStream.Read();
                break;
            }
            if (!Char.IsDigit(c))
            {
                throw new InvalidDataException("expected a digit value");
            }
            fileStream.Read();
            numStr += c;
        }

        if (numStr == "-0")
        {
            throw new InvalidDataException("value cannot be -0");
        }

        if (numStr.Length > 1 && numStr.StartsWith('0'))
        {
            throw new InvalidDataException("value cannot have a leading zero");
        }

        var num = Int64.Parse(numStr);
        return num;
    }

    private BEList ParseList()
    {
        if (fileStream.Peek() != 'l')
        {
            throw new InvalidDataException("expected value with format l<items>e");
        }
        fileStream.Read();

        var belist = new BEList();
        var listEnded = false;
        while (!listEnded)
        {
            var c = (char)fileStream.Peek();
            switch (c)
            {
                case 'e':
                    fileStream.Read();
                    listEnded = true;
                    break;

                case 'l':
                    var list = ParseList();
                    belist.Add(list);
                    break;

                case 'i':
                    var num = ParseInt();
                    belist.Add(num);
                    break;

                case var chr when Char.IsDigit(chr):
                    var bytestr = ParseByteString();
                    belist.Add(bytestr);
                    break;

                case 'd':
                    var d = ParseDictionary();
                    belist.Add(d);
                    break;

                default:
                    throw new InvalidOperationException("should be unreachable");
            }
        }

        return belist;
    }

    private BEDictionary ParseDictionary()
    {
        if (fileStream.Peek() != 'd')
        {
            throw new InvalidDataException("expect value with format d<key><value>e");
        }
        fileStream.Read();

        var dict = new BEDictionary();
        while (true)
        {
            var c = (char)fileStream.Peek();
            if (c == 'e')
            {
                break;
            }
            if (!Char.IsDigit(c))
            {
                throw new InvalidDataException("Dictionary key must be a bencoded string");
            }
            var key = ParseByteString();

            switch (c)
            {
                case 'l':
                    dict.Add(key, ParseList());
                    break;
                case 'd':
                    dict.Add(key, ParseDictionary());
                    break;
                case 'i':
                    dict.Add(key, ParseInt());
                    break;
                case var chr when Char.IsDigit(chr):
                    dict.Add(key, ParseByteString());
                    break;
                default:
                    throw new InvalidDataException("must be a valid bencoding type");
            }
        }
        return dict;
    }
}
