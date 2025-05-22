// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Xml.Linq;

namespace Microsoft.Build.Sql.Tests
{
    public static class ProjectUtils
    {
        /// <summary>
        /// Adds a PropertyGroup to the project XML and populates it with <paramref name="properties"/>.
        /// </summary>
        public static void AddProperties(string projectFilePath, IEnumerable<KeyValuePair<string, string>> properties)
        {
            using (ProjectCollection projectCollection = GetNewEngine())
            {
                Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);

                ProjectPropertyGroupElement propertyGroup = project.Xml.AddPropertyGroup();
                foreach (KeyValuePair<string, string> property in properties)
                {
                    propertyGroup.AddProperty(property.Key, property.Value);
                }

                project.Save(project.FullPath);
                projectCollection.UnloadAllProjects();
            }
        }

        /// <summary>
        /// Adds an ItemGroup to the project XML and populates it with <paramref name="itemName"/> and <paramref name="filePaths"/>.
        /// Result should look something like:
        ///   <ItemGroup>
        ///     <itemName Include="filePaths[0]" />
        ///     <itemName Include="filePaths[1]" />
        ///     ...
        ///   </ItemGroup>
        /// </summary>
        /// <param name="addMetadata">Optional delegate to set metadata on each item added.</param>
        public static void AddItemGroup(string projectFilePath, string itemName, string[] filePaths, Action<ProjectItemElement>? addMetadata = null)
        {
            if (filePaths != null && filePaths.Length > 0)
            {
                using (ProjectCollection projectCollection = GetNewEngine())
                {
                    Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);

                    ProjectItemGroupElement itemGroup = project.Xml.AddItemGroup();
                    foreach (string filePath in filePaths)
                    {
                        ProjectItemElement item = project.Xml.CreateItemElement(itemName, filePath);
                        itemGroup.AppendChild(item);
                        addMetadata?.Invoke(item);
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
        /// Sets metadata on an item in a project.
        /// </summary>
        /// <param name="projectFilePath">Path to the project file.</param>
        /// <param name="itemName">Name of the item type (e.g., "ProjectReference").</param>
        /// <param name="itemInclude">Include path of the item to set metadata on.</param>
        /// <param name="metadataName">Name of the metadata to set.</param>
        /// <param name="metadataValue">Value of the metadata to set.</param>
        public static void SetItemMetadata(string projectFilePath, string itemName, string itemInclude, string metadataName, string metadataValue)
        {
            using (ProjectCollection projectCollection = GetNewEngine())
            {
                Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);
                
                foreach (ProjectItemElement item in project.Xml.Items)
                {
                    if (item.ItemType == itemName && item.Include == itemInclude)
                    {
                        item.SetMetadataValue(metadataName, metadataValue);
                        break;
                    }
                }

                project.Save(project.FullPath);
                projectCollection.UnloadAllProjects();
            }
        }

        public static void AddTarget(string projectFilePath, string targetName, Action<ProjectTargetElement> targetAction)
        {
            using (ProjectCollection projectCollection = GetNewEngine())
            {
                Project project = new Project(projectFilePath, null, "Current", projectCollection, ProjectLoadSettings.IgnoreMissingImports);

                ProjectTargetElement target = project.Xml.AddTarget(targetName);
                targetAction(target);

                project.Save(project.FullPath);
                projectCollection.UnloadAllProjects();
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