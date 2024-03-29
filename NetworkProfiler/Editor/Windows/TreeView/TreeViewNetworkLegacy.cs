#if !UNITY_2022_1_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    class TreeViewNetwork : VisualElement
    {
        public enum DisplayType
        {
            Messages,
            Activity,
        }

        readonly TreeModel m_TreeModel;
        TreeView m_InnerTreeView;
        VisualElement m_TreeViewContainer;
        SortDirection m_SortDirection;

        public TreeViewNetwork(TreeModel treeModel)
        {
            m_TreeModel = treeModel;

            style.fontSize = 14;
            style.flexDirection = FlexDirection.Row;

            this.StretchToParentSize();
        }

        bool HasConnections => m_TreeModel?.Children.Count > 0;

        public void Show()
        {
            if (HasConnections)
            {
                BuildTreeView(m_SortDirection);
            }
        }

        void BuildTreeView(SortDirection sort)
        {
            var rootItems = SortAndStructureData(sort, m_TreeModel);
            UpdateTreeView(rootItems);
        }

        static List<ITreeViewItem> SortAndStructureData(SortDirection sort, TreeModel tree)
        {
            tree.SortChildren(sort);
            return CreateTreeViewItemsFromTreeData(tree);
        }

        void UpdateTreeView(IList<ITreeViewItem> rootItems)
        {
            m_InnerTreeView?.RemoveFromHierarchy();
            m_InnerTreeView = new TreeView(rootItems, 20, MakeItem, BindItem);

            foreach (var item in rootItems)
            {
                SetExpandedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
                SetSelectedStateRecursive(m_InnerTreeView, (TreeViewItem<IRowData>) item);
            }

            m_InnerTreeView.onExpandedStateChanged += UpdateFoldoutState;
            m_InnerTreeView.onItemsChosen += OnItemsChosen;
            m_InnerTreeView.onSelectionChange += OnSelectionChange;
            InitializeStylingTreeView(m_InnerTreeView);
            AddTreeView(m_InnerTreeView);
        }

        void OnItemsChosen(IEnumerable<ITreeViewItem> items)
        {
            foreach (var item in items)
            {
                var itemWithRow = item as TreeViewItem<IRowData>;
                itemWithRow?.data.OnSelectedCallback?.Invoke();
            }
        }

        void OnSelectionChange(IEnumerable<ITreeViewItem> items)
        {
            var filteredItems = items.OfType<TreeViewItem<IRowData>>().ToList();
            var pathList = filteredItems.Select(item => item.data.TreeViewPath).ToList();
            var networkIdList = filteredItems.Select(item => item.data.Id).ToList();
            DetailsViewPersistentState.SetSelected(pathList, networkIdList);
        }

        static void InitializeStylingTreeView(TreeView treeView)
        {
            treeView.selectionType = SelectionType.Multiple;

            treeView.style.flexGrow = 1f;
            treeView.style.flexShrink = 0f;
            treeView.style.flexBasis = 0f;
        }

        static void SetExpandedStateRecursive(TreeView treeView, TreeViewItem<IRowData> item)
        {
            var uniquePath = item.data.TreeViewPath + item.data.Id;
            var expandedState = DetailsViewPersistentState.IsFoldedOut(uniquePath);
            if (expandedState)
            {
                treeView.ExpandItem(item.id);
            }
            else
            {
                treeView.CollapseItem(item.id);
            }

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    SetExpandedStateRecursive(treeView, (TreeViewItem<IRowData>) child);
                }
            }
        }

        static void SetSelectedStateRecursive(TreeView treeView, TreeViewItem<IRowData> item)
        {
            /// Check if the item is selected by using both path and id to avoid conflicts
            var uniquePath = item.data.TreeViewPath + item.data.Id;
            var selectedState = DetailsViewPersistentState.IsSelected(uniquePath);
            if (selectedState)
            {
                treeView.AddToSelection(item.id);
            }
            else
            {
                treeView.RemoveFromSelection(item.id);
            }

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    SetSelectedStateRecursive(treeView, (TreeViewItem<IRowData>) child);
                }
            }
        }

        void FixSelectionAfterToggle()
        {
            m_InnerTreeView.Refresh();
            var idd = m_InnerTreeView.selectedItem;
            if (idd is not null)
            {
                m_InnerTreeView.SetSelectionIfVisible(idd.id);
            }
        }

        void UpdateFoldoutState(int id, bool state)
        {
            if (m_InnerTreeView.FindItem(id) is TreeViewItem<IRowData> item)
            {
                /// The unique path is used to identify the item in the persistent state
                var treeViewId = item.data.Id;
                var treeViewPath = item.data.TreeViewPath;
                var locator = treeViewPath + treeViewId;
                DetailsViewPersistentState.SetFoldout(locator, state);
            }

            FixSelectionAfterToggle();
        }

        void AddTreeView(TreeView treeView)
        {
            m_TreeViewContainer?.RemoveFromHierarchy();

            m_TreeViewContainer = new VisualElement
            {
                name = "TreeView Container"
            };
            m_TreeViewContainer.style.flexGrow = 1f;
            m_TreeViewContainer.style.flexShrink = 0f;
            m_TreeViewContainer.style.flexBasis = 0f;

            m_TreeViewContainer.Add(treeView);

            Add(m_TreeViewContainer);
        }

        static VisualElement MakeItem()
        {
            return new DetailsViewRow();
        }

        static void BindItem(VisualElement element, ITreeViewItem item)
        {
            (element as DetailsViewRow)?.BindItem(item);
        }

        static List<ITreeViewItem> CreateTreeViewItemsFromTreeData(TreeModel tree)
        {
            var nextId = 0;
            return tree.Children.Select(child => CreateTreeViewItemsRecursive(child, ref nextId)).ToList();
        }

        static ITreeViewItem CreateTreeViewItemsRecursive(TreeModelNode node, ref int incrementalId)
        {
            var item = new TreeViewItem<IRowData>(incrementalId++, node.RowData);
            foreach (var child in node.Children)
            {
                item.AddChild(CreateTreeViewItemsRecursive(child, ref incrementalId));
            }

            return item;
        }

        public void NameSort(bool isAscending)
        {
            if (!HasConnections)
            {
                return;
            }

            var sort = isAscending
                ? SortDirection.NameAscending
                : SortDirection.NameDescending;
            m_SortDirection = sort;
            BuildTreeView(sort);
        }

        public void TypeSort(bool isAscending)
        {
            if (!HasConnections)
            {
                return;
            }

            var sort = isAscending
                ? SortDirection.TypeAscending
                : SortDirection.TypeDescending;
            m_SortDirection = sort;
            BuildTreeView(sort);
        }

        public void BytesSentSort(bool isAscending)
        {
            if (!HasConnections)
            {
                return;
            }

            var sort = isAscending
                ? SortDirection.BytesSentAscending
                : SortDirection.BytesSentDescending;
            m_SortDirection = sort;
            BuildTreeView(sort);
        }

        public void BytesReceivedSort(bool isAscending)
        {
            if (!HasConnections)
            {
                return;
            }

            var sort = isAscending
                ? SortDirection.BytesReceivedAscending
                : SortDirection.BytesReceivedDescending;
            m_SortDirection = sort;
            BuildTreeView(sort);
        }

        public void RefreshSelected()
        {
            if (m_InnerTreeView != null)
            {
                BuildTreeView(m_SortDirection);
            }
        }
    }
}
#endif
