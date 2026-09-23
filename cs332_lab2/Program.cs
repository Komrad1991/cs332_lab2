using cs332_lab2;

class Program
{
   static void Main(string[] args)
   {
      string path = args.Length > 0 ? args[0] : "../../../Alice.png";
      task2.Run(path);
   }
}
