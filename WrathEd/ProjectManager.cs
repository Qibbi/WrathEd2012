using System;

namespace WrathEd
{
	public class ProjectManager
	{
		public string ProjectName { get; set; }
		public string ContentPath { get; set; }
		public WrathEdXML.Project.Project Project { get; set; }
		public WrathEdTreeViewItem Tree { get; set; }

		public ProjectManager(WrathEdXML.Project.Project project, string path)
		{
			ProjectName = project.id;
			ContentPath = path;
			Project = project;
		}

		public void Save()
		{
			//Globals.MainWindow.BeginSave();
			Project.Save(ContentPath);
			//Globals.MainWindow.SetReady();
		}
	}
}
