using Flamui;
using Regionalmeisterschaften;
using SwissSkillsTraining1;

var windowHost = new FlamuiWindowHost();

Store store = new();

windowHost.CreateWindow("SwissSkills", ui => MainWindow.MainApp(ui, store, windowHost));

windowHost.Run();