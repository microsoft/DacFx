// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;

namespace Microsoft.Build.Sql.Tests
{
    public static class ProjectUtils
    {
        /// <summary>
        /// Adds an ItemGroup to the project XML and populates it with <paramref name="itemName"/> and <paramref name="filePaths"/>.
        /// Result should look something like:
        ///   <ItemGroup>
        ///     <itemName Include="filePaths[0]" />
        ///     <itemName Include="filePaths[1]" />
        ///     ...
        ///   </ItemGroup>
        /// </summary>
        public static void AddItemGroup(string projectFilePath, string itemName, string[] filePaths)
        {
            if (filePaths != null && filePaths.Length > 0)
            {
                using (ProjectCollection projectCollection = GetNewEngine())
                {
                    Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);

                    ProjectItemGroupElement itemGroup = project.Xml.AddItemGroup();
                    foreach (string filePath in filePaths)
                    {
                        itemGroup.AddItem(itemName, filePath);
                    }

                    project.Save(project.FullPath);
                    projectCollection.UnloadAllProjects();
                }
            }
        }

        /// <summary>
        /// Adds an ItemGroup to the project XML and populates it with remove <paramref name="itemName"/> and <paramref name="filePaths"/>.
        /// Result should look something like:
        ///   <ItemGroup>
        ///     <itemName Remove="filePaths[0]" />
        ///     <itemName Remove="filePaths[1]" />
        ///     ...
        ///   </ItemGroup>
        /// </summary>
        public static void AddItemRemoveGroup(string projectFilePath, string itemName, string[] filePaths)
        {
            if (filePaths != null && filePaths.Length > 0)
            {
                using (ProjectCollection projectCollection = GetNewEngine())
                {
                    Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);

                    ProjectItemGroupElement itemGroup = project.Xml.AddItemGroup();
                    foreach (string filePath in filePaths)
                    {
                        var removeItem = project.Xml.CreateItemElement(itemName);
                        removeItem.Remove = filePath;
                        itemGroup.AppendChild(removeItem);
                    }

                    project.Save(project.FullPath);
                    projectCollection.UnloadAllProjects();
                }
            }
        }

        /// <summary>
        /// Gets the target platform value for the sql project
        /// </summary>
        /// TODO update to use dacfx SQL project APIs
        public static string GetTargetPlatform(string projectFilePath)
        {
            string dspValue = "";
            // parse file xml to <DSP> element in <Project> root element
            XDocument sqlproj = XDocument.Load(projectFilePath);
            XElement? dsp = sqlproj.Root?.Element("PropertyGroup")?.Element("DSP");
            if (dsp != null)
            {
                dspValue = dsp.Value;
            }
            return dspValue;
        }

        public static ProjectCollection GetNewEngine()
        {
            ProjectCollection msbuild = new ProjectCollection();
            msbuild.OnlyLogCriticalEvents = false;
            return msbuild;
        }
    }
}