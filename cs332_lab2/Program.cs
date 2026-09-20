using cs332_lab2;

class Program
{
    static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : "ФРУКТЫ.jpg";
        task1.Run(path);
    }
}
