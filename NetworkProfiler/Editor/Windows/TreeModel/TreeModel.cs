﻿using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    class TreeModel
    {
        readonly List<TreeModelNode> m_Children = new List<TreeModelNode>();
        
        /// <summary>
        /// Checks if there is upcoming info to show on DetailsView
        /// </summary>
        internal bool HasData => Children.Count > 0;
        public IReadOnlyList<TreeModelNode> Children => m_Children;

        public void SortChildren(SortDirection direction)
        {
            m_Children.Sort((a,b) => RowDataSorting.SortOperation(a.RowData, b.RowData, direction));
            foreach (var child in Children)
            {
                child.SortChildren(direction);
            }
        }

        public void AddChild(TreeModelNode node)
        {
            m_Children.Add(node);
        }
    }
}
