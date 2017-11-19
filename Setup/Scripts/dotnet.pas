// Check .NET Framework
function IsDotNetDetected(version: string; release: cardinal): boolean;

var 
    reg_key: string; 		// Просматриваемый подраздел системного реестра
    success: boolean; 		// Флаг наличия запрашиваемой версии .NET
    release45: cardinal; 	// Номер релиза для версии 4.5.x
    key_value: cardinal; 	// Прочитанное из реестра значение ключа
    sub_key: string;

begin

    success := false;
    reg_key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\';
    
    // Вресия 3.0
    if Pos('v3.0', version) = 1 then
      begin
          sub_key := 'v3.0';
          reg_key := reg_key + sub_key;
          success := RegQueryDWordValue(HKLM, reg_key, 'InstallSuccess', key_value);
          success := success and (key_value = 1);
      end;

    // Вресия 3.5
    if Pos('v3.5', version) = 1 then
      begin
          sub_key := 'v3.5';
          reg_key := reg_key + sub_key;
          success := RegQueryDWordValue(HKLM, reg_key, 'Install', key_value);
          success := success and (key_value = 1);
      end;

     // Вресия 4.0 клиентский профиль
     if Pos('v4.0 Client Profile', version) = 1 then
      begin
          sub_key := 'v4\Client';
          reg_key := reg_key + sub_key;
          success := RegQueryDWordValue(HKLM, reg_key, 'Install', key_value);
          success := success and (key_value = 1);
      end;

     // Вресия 4.0 расширенный профиль
     if Pos('v4.0 Full Profile', version) = 1 then
      begin
          sub_key := 'v4\Full';
          reg_key := reg_key + sub_key;
          success := RegQueryDWordValue(HKLM, reg_key, 'Install', key_value);
          success := success and (key_value = 1);
      end;

     // Вресия 4.5
     if Pos('v4.5', version) = 1 then
      begin
          sub_key := 'v4\Full';
          reg_key := reg_key + sub_key;
          success := RegQueryDWordValue(HKLM, reg_key, 'Release', release45);
          success := success and (release45 >= release);
      end;
        
    result := success;

end;

function IsRequiredDotNetDetected(): boolean;
begin
    result := IsDotNetDetected('v4.5', 0);
end;

function InitializeSetup(): boolean;
begin

  // Если нет тербуемой версии .NET выводим сообщение о том, что инсталлятор
  // попытается установить её на данный компьютер
  if not IsDotNetDetected('v4.5', 0) then
    begin
      MsgBox('{#MyAppName} requires Microsoft .NET Framework 4.5.'#13#13
             'The installer will attempt to install it', mbInformation, MB_OK);
    end;   

  result := true;
end;