string key = "", bool skipCheck = false
instance.UseFullMouseEmulation = false;
string address = "";
while (true)
{
instance.CloseExtraTabs();
instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
instance.CloseExtraTabs(); var toDo = ""; Thread.Sleep(3000);
DateTime deadline = DateTime.Now.AddSeconds(60);//if (DateTime.Now < deadline ) Thread.Sleep(1000);
var password = SAFU.HWPass(project);

while (true)
{Thread.Sleep(3000);
if (!instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid) 
    {toDo = "install,import";break;}
else if (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0).IsVoid) 
    {toDo = "checkAddress";break;}
else if (!instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0).IsVoid) 
    {toDo = "import";break;}
else if (!instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0).IsVoid) 
    {toDo = "unlock";break;}
}
Loggers.W3Log(project,toDo);

if ( toDo.Contains("install")) 
{
    string path = $"{project.Path}.crx\\MetaMask 11.16.0.crx"; 
    instance.InstallCrxExtension(path);
    Loggers.W3Log(project,$"installing {path}"); 
}

if (toDo.Contains("import")) 
{
    string welcomeURL = $"chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html#onboarding/welcome"; 
    while (true)
    {
        if (instance.ActiveTab.URL == welcomeURL) break;
        if (DateTime.Now < deadline ) Thread.Sleep(1000);
        else
        {
            instance.CloseExtraTabs();
            instance.ActiveTab.Navigate("chrome-extension://nkbihfbeogaeaoehlefnkodbefgpgknn/home.html", "");
            break;
        }
    }
    if (key == "") key = SQL.DBget(project,"KeyEvm");
    else skipCheck = true;
    
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("h2", "innertext", "Let\'s\\ get\\ started", "regexp", 0));
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "innertext", "I\\ agree\\ to\\ MetaMask\'s\\ Terms\\ of\\ use", "regexp", 1),10,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "aria-label", "Close", "regexp", 0));
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "onboarding-create-wallet", "regexp", 0),10,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "metametrics-no-thanks", "regexp", 0),10,0);
    instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "data-testid", "create-password-new", "regexp", 0),password);
    instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "data-testid", "create-password-confirm", "regexp", 0),password);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "innertext", "I\\ understand\\ that\\ MetaMask\\ cannot\\ recover\\ this\\ password\\ for\\ me.\\ Learn\\ more", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "create-password-wallet", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "secure-wallet-later", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("label", "class", "skip-srp-backup-popover__label", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "skip-srp-backup", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "onboarding-complete-done", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "pin-extension-next", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "pin-extension-done", "regexp", 0),5,0);
    Thread.Sleep(1000); while (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
    {instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "popover-close", "regexp", 0));}
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-menu-icon", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "multichain-account-menu-popover-action-button", "regexp", 0),5,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("span", "style", "mask-image:\\ url\\(\"./images/icons/import.svg\"\\);", "regexp", 0),5,0);
    instance.WaitSetValue(() => instance.ActiveTab.FindElementById("private-key-box"), key);
    instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "import-account-confirm-button", "regexp", 0),5,0);
    toDo = "checkAddress";
}


if ( toDo == "unlock") 
{
    //pass
    instance.WaitSetValue(() => instance.ActiveTab.FindElementById("password"),password,3,0);
    instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0));
    if (!instance.ActiveTab.FindElementByAttribute("p", "innertext", "Incorrect password", "text", 0).IsVoid) 
    {
        instance.CloseAllTabs(); instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
        project.Variables["a0debug"].Value = $"wallet fuckup"; project.SendWarningToLog(Loggers.W3Log(project),true); throw new Exception("wrongPassword");
    }	
    toDo = "checkAddress";
}

if ( toDo == "checkAddress") 
{
        while (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).IsVoid)
        {
            try	{instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "popover-close", "regexp", 0),2,0);}
            catch{instance.ActiveTab.FindElementByAttribute("button", "innertext", "Got\\ it", "regexp", 0).RiseEvent("click", instance.EmulationLevel);};
        }
        
        //addres
        try{
        instance.WaitSetValue(() => instance.ActiveTab.FindElementById("password"),password,3,0);
        instance.WaitClick(() =>  instance.ActiveTab.FindElementByAttribute("button", "data-testid", "unlock-submit", "regexp", 0));}
        catch{}
        
        instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-options-menu-button", "regexp", 0),5,0);
        instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "data-testid", "account-list-menu-details", "regexp", 0),5,0);
        address = instance.WaitGetValue(() =>    instance.ActiveTab.FindElementByAttribute("button", "data-testid", "address-copy-button-text", "regexp", 0));
        
        if (!skipCheck)
        if(!String.Equals(address,project.Variables["addressEvm"].Value,StringComparison.OrdinalIgnoreCase))
        {
            instance.CloseAllTabs(); instance.UninstallExtension("nkbihfbeogaeaoehlefnkodbefgpgknn"); 
            Loggers.W3Log(project,$"!WrongWallet expected: {project.Variables["addressEvm"].Value}. InWallet {address}"); continue;//throw new Exception("!WrongWallet");
        }	
}
instance.UseFullMouseEmulation = true;
return address;
