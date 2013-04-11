// Smart API - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IContentClassElement : IWorkflowAssignable, IAttributeContainer
    {
        /// <summary>
        ///     Element category of the lement
        /// </summary>
        ContentClassCategory Category { get; }

        /// <summary>
        ///     Save element on the server. Saves only the attributes!
        /// </summary>
        void Commit();

        IContentClass ContentClass { get; set; }

        /// <summary>
        ///     Language variant of the element.
        /// </summary>
        ILanguageVariant LanguageVariant { get; }

        /// <summary>
        ///     TypeId of the element.
        /// </summary>
        ElementType Type { get; }

        /// <summary>
        ///     Copies the element to another content class by creating a new element and copying the attribute values to it.
        ///     Make sure to set the language variant in the target project into which the element should be copied, first.
        /// </summary>
        /// <param name="contentClass"> target content class, into which the element should be copied </param>
        /// <returns> the created copy </returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>Override this method, if you need to set other values than the direct attributes of the element (e.g. setting text values of TextHtml elements)</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The target content class is only modified on the server, thus the content class object does not contain the newly created element.
        ///                 If you need an updated version of the content class, you have to retrieve it again with
        ///                 <code>new ContentClass(Project, Guid);</code>
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        IContentClassElement CopyToContentClass(IContentClass contentClass);
    }
}