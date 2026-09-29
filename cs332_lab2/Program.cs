using cs332_lab2;

class Program
{
   static void Main(string[] args)
   {
      string path1 = args.Length > 0 ? args[0] : "ФРУКТЫ.jpg";
      task1.Run(path1);
      
      string path2 = args.Length > 0 ? args[0] : "../../../Alice.png";
      task2.Run(path2);

      string path3 = "images.jpg";
      task3.Run(path3, 40, 2, 2);
    }
}
