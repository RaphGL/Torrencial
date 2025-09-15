namespace Torrencial.Bencode;

public class BEDictionary {
    
}

public class BEList
{
    private List<object> items = new List<object>();

    public BEList() { }

    public void Add(Int64 item) => items.Add(item);
    public void Add(string item) => items.Add(item);
    // TODO: use custom dictionary instead
    public void Add(Dictionary<int, int> item) => items.Add(item);
    public void Add(BEList item) => items.Add(item);
}

public class Parser
{
    private StreamReader fileStream;

    public Parser(string path)
    {
        fileStream = new StreamReader(path);
    }

    public string Parse()
    {
        var str = ParseByteString();
        var i64 = ParseInt();
        Console.WriteLine(i64);
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
            var c = fileStream.Peek();
            switch (c)
            {
                case 'e':
                    fileStream.Read();
                    listEnded = true;
                    break;

                case 'l':
                    break;

                case 'i':
                    var num = ParseInt();
                    belist.Add(num);
                    break;

                case var chr when Char.IsDigit((char)chr):
                    var bytestr = ParseByteString();
                    belist.Add(bytestr);
                    break;

                case 'd':
                    // TODO
                    break;

                default:
                    throw new InvalidOperationException("should be unreachable");
            }
        }

        return belist;
    }

    private void ParseDictionary() {
        if (fileStream.Peek() != 'd') {
            throw new InvalidOperationException("expect value with format d<key><value>e");
        }
        fileStream.Read();

        
    }
}
