﻿using System;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal abstract class ViewModelBase : IRowData
    {
        /// <param name="parent"/>
        /// <param name="name"/>
        /// <param name="metricType"/>
        /// <param name="onSelectedCallback"/>
        /// <param name="connection"/>
        /// <param name="localConnection"/>
        /// <param name="treeViewPathComponent">
        /// Overrides this nodes path component within the tree. When this value is null `name` is used instead
        /// </param>
        protected ViewModelBase(
            IRowData parent,
            string name,
            MetricType metricType,
            Action onSelectedCallback,
            ulong id = 0,
            ConnectionInfo? connection = null,
            ConnectionInfo? localConnection = null,
            string treeViewPathComponent = null)
            : this (
                parent,
                name,
                metricType.GetDisplayNameString(),
                metricType.GetTypeNameString(),
                onSelectedCallback,
                id,
                connection,
                localConnection,
                treeViewPathComponent)
        {
        }

        /// <param name="id"/>
        /// <param name="parent"/>
        /// <param name="name"/>
        /// <param name="typeDisplayName"/>
        /// <param name="typeName"/>
        /// <param name="onSelectedCallback"/>
        /// <param name="connection"/>
        /// <param name="localConnection"/>
        /// <param name="treeViewPathComponent">
        /// Overrides this nodes path component within the tree. When this value is null `name` is used instead
        /// </param>
        protected ViewModelBase(
            IRowData parent,
            string name,
            string typeDisplayName,
            string typeName,
            Action onSelectedCallback,
            ulong id = 0,
            ConnectionInfo? connection = null,
            ConnectionInfo? localConnection = null,
            string treeViewPathComponent = null)
        {
            Id = id;
            Parent = parent;
            Connection = connection ?? (parent as ViewModelBase)?.Connection ?? new ConnectionInfo();
            LocalConnection = localConnection ?? (parent as ViewModelBase)?.LocalConnection ?? new ConnectionInfo();
            TreeViewPath = Parent?.TreeViewPath + (treeViewPathComponent ?? name);
            Name = name;
            TypeDisplayName = typeDisplayName;
            TypeName = typeName;
            OnSelectedCallback = onSelectedCallback;
        }

        public ulong Id { get; }
        public string Name { get; }

        public string TypeDisplayName { get; }

        public string TypeName { get; }

        public IRowData Parent { get; }

        public string TreeViewPath { get; }

        public Action OnSelectedCallback { get; }

        public BytesSentAndReceived Bytes { get; set; }

        public ConnectionInfo Connection { get; }

        public ConnectionInfo LocalConnection { get; }

        public bool SentOverLocalConnection => Connection == LocalConnection;
    }
}
