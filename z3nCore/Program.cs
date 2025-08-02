

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.ProjectModel;




namespace ZBS //by @w3bgrep
{
    public class Program : IZennoExternalCode
    {
        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            int executionResult = 0;

            return executionResult;
        }
    }


}
