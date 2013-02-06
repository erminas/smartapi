// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Possible types of elements
    /// </summary>
    public enum ElementType
    {
        None = 0,
        // Content Elements
        Background = 19,
        DatabaseContent = 14,
        Image = 2,
        Ivw = 100,
        ListEntry = 25,
        Media = 38,
        OptionList = 8,
        ProjectContent = 10,
        ConditionRedDotLiveServer = 1004,
        StandardFieldText = 1,
        StandardFieldDate = 5,
        StandardFieldTime = 39,
        StandardFieldNumeric = 48,
        StandardFieldUserDefined = 999,
        StandardFieldEmail = 50,
        StandardFieldUrl = 51,
        StandardFieldTextLegacy = 1000,
        TextAscii = 31,
        TextHtml = 32,
        Transfer = 60,
        XcmsProjectContentElement = 98,
        Headline = 12,

        // Structure Elements
        AnchorAsText = 26,
        AnchorAsImage = 27,
        AnchorNotYetDefinedAsTextOrImage = 2627,
        Area = 15,
        Browse = 23,
        Container = 28,
        Frame = 3,
        List = 13,
        SiteMap = 99,
        HitList = 24,

        // Meta Elements
        Attribute = 1003,
        Info = 1002
    };
}