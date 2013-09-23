// SmartAPI - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Workflows
{
    public interface IWorkFlowAction : IRedDotObject, IProjectObject
    {
        string Path { get; }
        WorkflowActionType Type { get; set; }
    }

    //todo implement correctly
    internal class WorkFlowAction : RedDotProjectObject, IWorkFlowAction
    {
        private WorkflowActionType _type;

        internal WorkFlowAction(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        internal WorkFlowAction(IProject project, Guid guid) : base(project, guid)
        {
        }

        public string Path
        {
            get
            {
                string path = null;
                if (_type == WorkflowActionType.WriteWorkflowXmlFile)
                {
                    path = XmlElement.GetAttributeValue("path");
                }
                return path;
            }
        }

        public WorkflowActionType Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }

    public enum WorkflowActionType
    {
        None = 0,
        ContentWorkflow = 1115,
        StructureWorkflow = 1116,
        PageCreated_Action = 1120,
        PageChanged_Action = 1125,
        PageDeleted_Action = 1130,
        PageConnectedToLink_Action = 1140,
        PageDisconnectedFromLink_Action = 1145,
        ReleasePage_Reaction = 1155,
        ReleaseByWebComplianceManager_Reaction = 1156,
        ReleaseOfAStructure_Reaction = 1157,
        EmailNotification_Reaction = 1170,
        PageForwarding_Reaction = 1175,
        PageEscalated_Action = 1177,
        StartPublication_Reaction = 1178,
        PageReleased_Action = 1185,
        PageRejected_Action = 1190,
        PageTransferredToOtherLanguageVariants_Reaction = 1200,
        AutomaticResubmission_Reaction = 1205,
        PageTranslated_Action = 1210,
        AssignKeywordToPage_Reaction = 1310,
        AssignKeywordToStructureElement_Reaction = 1315,
        PageAttachedToStructure_Reaction = 1340,
        PageDisconnectedFromStructure_Reaction = 1345,
        WriteWorkflowXmlFile = 1225
    };
}