using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n.Conf
{
    public static class AppReg
    {
        public static IServiceCollection AddApp(this IServiceCollection services, Instance instance, IZennoPosterProjectModel zModel)
        {
            services
                .AddScoped<Instance>(opt => instance)
                .AddScoped<IZennoPosterProjectModel>(opt => zModel)
                .AddScoped<HW>();
            return services;
      
        
        }
    }

    internal class AppRun
    {
        private static IHostBuilder CreateHost(Instance instance, IZennoPosterProjectModel zModel) => Host
            .CreateDefaultBuilder().
            ConfigureServices(services => services.AddApp(instance, zModel));

        public static void Execute(Instance instance, IZennoPosterProjectModel project)
        {
            var host = CreateHost(instance,project).Build ();
            var HWS = host.Services.GetRequiredService<HW>();
            HWS.Run();
            return;

        }
   
    }

    public class HW
    {
        private readonly IZennoPosterProjectModel _model;
        public HW(IZennoPosterProjectModel model)
        {
            _model = model;
        }
        public void Run()
        {
            _model.SendInfoToLog("YoMan!");
        }

    }










}


