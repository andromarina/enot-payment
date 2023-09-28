
namespace EnotPayment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;            
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);

            CreateWebHostBuilder(args).Build().Run();

        }

        static void MyExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("MyExceptionHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseStartup<Startup>()
                        .UseUrls("http://localhost:4002");
                });
    }

}

