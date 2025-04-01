instance.CloseExtraTabs(); 
DateTime deadline = DateTime.Now.AddSeconds(60);
DateTime deadline2 = DateTime.Now.AddSeconds(108);
var backpackPopout = "chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html";
var backpackWelcome = "chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/options.html?onboarding=true";
var password = HWPass(project); var toDo = "";
DateTime deadline2 = DateTime.Now.AddSeconds(108);

instance.ActiveTab.Navigate(backpackPopout, ""); 
while (true)
{
    if (DateTime.Now < deadline2 ) W3Throw(project, $"backpack looped more than {deadline2}");
    if (DateTime.Now < deadline ) Thread.Sleep(3000);
    else
    {
        instance.CloseExtraTabs();
        instance.ActiveTab.Navigate(backpackPopout, "");
        Continue;
    }
    
    if (!instance.ActiveTab.FindElementByAttribute("div", "class", "error-code", "regexp", 0).IsVoid) 
		{toDo = "install,import"; break;}
	if (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Create\\ a\\ new\\ wallet", "regexp", 0).IsVoid)
		{toDo = "import"; break;}
	if (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Unlock", "regexp", 0).IsVoid)
		{toDo = "unlock"; break;}
	if (!instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).IsVoid)
		{toDo = ""; break;}		
}

if (project.Variables["accAddressSOL"].Value == "") toDo += ", getAddress";
project.SendInfoToLog(toDo,true);

if ( toDo.Contains("install")) 
{
	string path = $"{project.Path}.crx\\Backpack0.10.94.crx"; 
	instance.InstallCrxExtension(path);
	w3Log(project,$"installing {path}"); 
}



if (toDo.Contains("import")) 
{
	var key = KeySOL(project);
	if (instance.ActiveTab.URL != backpackWelcome) instance.ActiveTab.Navigate(backpackWelcome, "");
	while (true)
	{
		if (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0).IsVoid) 
			{ 
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ Wallet", "regexp", 0),10,0);
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("div", "class", "_dsp-flex\\ _ai-stretch\\ _fd-row\\ _fb-auto\\ _bxs-border-box\\ _pos-relative\\ _mih-0px\\ _miw-0px\\ _fs-0\\ _btc-889733467\\ _brc-889733467\\ _bbc-889733467\\ _blc-889733467\\ _w-10037\\ _pt-1316333121\\ _pr-1316333121\\ _pb-1316333121\\ _pl-1316333121\\ _gap-1316333121", "regexp", 0),5,0);
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import\\ private\\ key", "regexp", 0),5,0);
				instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("textarea", "fulltagname", "textarea", "regexp", 0),(key),5,0);
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Import", "regexp", 0),5,0);
				instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "placeholder", "Password", "regexp", 0),(HWPass(project)),5,0);
				instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "placeholder", "Confirm\\ Password", "regexp", 0),(HWPass(project)),5,0);
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("input:checkbox", "class", "PrivateSwitchBase-input\\ ", "regexp", 0),5,0);
				instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Next", "regexp", 0),5,0);
				if (!instance.ActiveTab.FindElementByAttribute("button", "innertext", "Open\\ Backpack", "regexp", 0).IsVoid) project.SendInfoToLog("backback imported");
				break;
			}
		if (!instance.ActiveTab.FindElementByAttribute("p", "innertext", "Already\\ setup", "regexp", 0).IsVoid) 
		{
			instance.ActiveTab.Navigate("chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html", "");
			toDo = "unlock"; break;
		}
	}
}



if (toDo.Contains("unlock")) 
{
	instance.WaitSetValue(() => instance.ActiveTab.FindElementByAttribute("input:password", "fulltagname", "input:password", "regexp", 0),(HWPass(project)),10,0);
	instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "innertext", "Unlock", "regexp", 0),5,0);
}

try{
string heToWait = instance.WaitGetValue(() => 
    instance.ActiveTab.FindElementByAttribute("p", "innertext", "already\\ been\\ setup.", "regexp", 0),3);
	instance.ActiveTab.Navigate("chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html", "");
//break;
}
catch{}

if (toDo.Contains("getAddress")) 
{w3Log(project,$"getting address");
	

	instance.ActiveTab.Navigate("chrome-extension://aflkmfhebedbjioipglgcbcmnbpgliof/popout.html", "");
	while (instance.ActiveTab.FindElementByAttribute("button", "class", "is_Button\\ ", "regexp", 0).IsVoid) 
		instance.ActiveTab.FindElementByAttribute("path", "d", "M12 5v14", "text", 0).RiseEvent("click", instance.EmulationLevel);
	
	int i = 0;	while (!instance.ActiveTab.FindElementByAttribute("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", i).IsVoid)i++;
	var publicSOL = instance.ActiveTab.FindElementByAttribute("p", "class", "MuiTypography-root\\ MuiTypography-body1", "regexp", i - 1 ).GetAttribute("innertext");
		instance.WaitClick(() => instance.ActiveTab.FindElementByAttribute("button", "aria-label", "TabsNavigator,\\ back", "regexp", 0));

	
	project.Variables["addressSol"].Value = publicSOL;
	Db.UpdAddressSol(project);
	//w3Query(project,$"UPDATE accBlockchain SET publicSOL = '{publicSOL}' WHERE acc0='{project.Variables["acc0"].Value}';",true); 
}
instance.CloseExtraTabs();



