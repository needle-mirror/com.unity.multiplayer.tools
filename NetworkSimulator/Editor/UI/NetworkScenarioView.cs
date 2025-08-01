﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Editor.UI
{
    class NetworkScenarioView : VisualElement
    {
        const string k_Uxml = "Packages/com.unity.multiplayer.tools/NetworkSimulator/Editor/UI/NetworkScenarioView.uxml";
        const string k_None = "None";
        const string k_StartString = "Start";
        const string k_PauseString = "Pause";
        const string k_ResumeString = "Resume";

        readonly Runtime.NetworkSimulator m_NetworkSimulator;
        readonly SerializedProperty m_NetworkScenarioProperty;
        readonly SerializedProperty m_SettingsFolded;
        readonly List<Type> m_Types;

        // Cached VisualElements:
        DropdownField m_DropdownField;
        Toggle m_AutoRunToggle;
        Foldout m_SettingsFoldout;
        NetworkScenarioSettings m_SettingsContainer;
        Button m_PauseResumeButton;

        // VisualElements Access:
        DropdownField ScenarioDropdown => m_DropdownField ??= this.Q<DropdownField>(nameof(ScenarioDropdown));
        Toggle AutoRunToggle => m_AutoRunToggle ??= this.Q<Toggle>(nameof(AutoRunToggle));
        Foldout SettingsFoldout => m_SettingsFoldout ??= this.Q<Foldout>(nameof(SettingsFoldout));
        NetworkScenarioSettings SettingsContainer => m_SettingsContainer ??= this.Q<NetworkScenarioSettings>(nameof(SettingsContainer));
        Button PauseResumeButton => m_PauseResumeButton ??= this.Q<Button>(nameof(PauseResumeButton));

        public NetworkScenarioView(SerializedObject serializedObject, Runtime.NetworkSimulator networkSimulator)
        {
            m_NetworkSimulator = networkSimulator;
            m_NetworkScenarioProperty = serializedObject.FindProperty(nameof(m_NetworkSimulator.m_Scenario));
            m_SettingsFolded = serializedObject.FindProperty(nameof(m_NetworkSimulator.m_IsScenarioSettingsFolded));

            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml).CloneTree(this);
            SettingsContainer.BindProperty(serializedObject, networkSimulator);

            m_Types = new(NetworkScenarioTypesLibrary.Types);
            List<string> choices = new() { k_None };
            choices.AddRange(m_Types.Select(x => ObjectNames.NicifyVariableName(x.Name)));

            ScenarioDropdown.choices = choices;
            UpdateScenarioDropdown();

            AutoRunToggle.BindProperty(serializedObject.FindProperty(nameof(networkSimulator.AutoRunScenario)));
            AutoRunToggle.SetEnabled(!Application.isPlaying);

            SetPauseResumeButton(m_NetworkSimulator.scenarioPlaybackState);

            this.AddEventLifecycle(OnAttach, OnDetach);
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            ScenarioDropdown.RegisterCallback<ChangeEvent<string>>(OnScenarioPresetSelected);
            SettingsFoldout.RegisterCallback<ChangeEvent<bool>>(OnSettingsFoldoutChange);
            PauseResumeButton.RegisterCallback<ClickEvent>(OnPauseResumeButtonClicked);
            EditorApplication.playModeStateChanged += OnPlayStateChanged;

            Undo.undoRedoPerformed += UndoRedoPerformed;
            
            m_NetworkSimulator.ScenarioPlaybackStateChangedEvent += OnScenarioPlaybackStateChanged;
            m_NetworkSimulator.ScenarioChangedEvent += OnScenarioChanged;
        }

        void OnDetach(DetachFromPanelEvent evt)
        {
            ScenarioDropdown.UnregisterCallback<ChangeEvent<string>>(OnScenarioPresetSelected);
            SettingsFoldout.UnregisterCallback<ChangeEvent<bool>>(OnSettingsFoldoutChange);
            PauseResumeButton.UnregisterCallback<ClickEvent>(OnPauseResumeButtonClicked);
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            
            m_NetworkSimulator.ScenarioPlaybackStateChangedEvent -= OnScenarioPlaybackStateChanged;
            m_NetworkSimulator.ScenarioChangedEvent -= OnScenarioChanged;
        }

        void UndoRedoPerformed()
        {
            UpdateScenarioDropdown();
        }

        void OnSettingsFoldoutChange(ChangeEvent<bool> evt)
        {
            m_SettingsFolded.boolValue = evt.newValue;
            m_NetworkScenarioProperty.serializedObject.ApplyModifiedProperties();
        }

        void OnScenarioPresetSelected(ChangeEvent<string> changeEvent)
        {
            if (changeEvent.newValue == k_None)
            {
                m_NetworkSimulator.Scenario = null;
                return;
            }

            var selectedIndexWithoutNone = m_DropdownField.index - 1;
            // We should never get an index lower than 0 as if we select None it will be catch by the if above, thus the assert here if we ever change the code above.
            Debug.Assert(selectedIndexWithoutNone >= 0);

            var selectedType = m_Types[selectedIndexWithoutNone];
            var networkScenario = NetworkScenarioTypesLibrary.GetInstanceForTypeName(selectedType.Name);
            m_NetworkSimulator.UsedEditorGUI = true;
            m_NetworkSimulator.Scenario = networkScenario;
        }

        void UpdateScenarioDropdown()
        {
            var hasSettings = m_NetworkSimulator.Scenario != null;

            if (hasSettings)
            {
                ScenarioDropdown.SetValueWithoutNotify(ObjectNames.NicifyVariableName(m_NetworkSimulator.Scenario.GetType().Name));
                SettingsFoldout.style.display = DisplayStyle.None;
                PauseResumeButton.SetEnabled(true);
            }
            else
            {
                ScenarioDropdown.index = 0;
                SettingsFoldout.style.display = DisplayStyle.Flex;
                SettingsFoldout.value = m_SettingsFolded.boolValue;
                PauseResumeButton.SetEnabled(false);
            }
        }

        void OnPauseResumeButtonClicked(ClickEvent evt)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            var scenario = m_NetworkSimulator.Scenario;

            if (scenario == null)
            {
                return;
            }

            m_NetworkSimulator.UsedEditorGUI = true;
            scenario.IsPaused = !scenario.IsPaused;
            SetPauseResumeButton(scenario.IsPaused);
        }

        void SetPauseResumeButton(bool isPaused)
        {
            SetPauseResumeButtonString(isPaused ? k_ResumeString : k_PauseString);
        }
        
        void SetPauseResumeButton(Runtime.NetworkSimulator.ScenarioPlaybackState state)
        {
            switch (state)
            {
                case Runtime.NetworkSimulator.ScenarioPlaybackState.Initial:
                    SetPauseResumeButtonString(k_StartString);
                    break;
                case Runtime.NetworkSimulator.ScenarioPlaybackState.Paused:
                    SetPauseResumeButtonString(k_ResumeString);
                    break;
                case Runtime.NetworkSimulator.ScenarioPlaybackState.Running:
                    SetPauseResumeButtonString(k_PauseString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, "Not supported type");
            }
        }
        
        void SetPauseResumeButtonString(string text)
        {
            PauseResumeButton.text = text;
        }

        void OnPlayStateChanged(PlayModeStateChange playModeStateChange)
        {
            AutoRunToggle.SetEnabled(!Application.isPlaying);
        }
        
        void OnScenarioPlaybackStateChanged(Runtime.NetworkSimulator.ScenarioPlaybackState newState)
        {
            SetPauseResumeButton(newState);
        }
        
        void OnScenarioChanged(NetworkScenario newScenario)
        {
            if (newScenario == null)
            {
                SettingsContainer.DeselectScenario();
            }
            else
            {
                SettingsContainer.SelectScenario(newScenario);
            }
            
            UpdateScenarioDropdown();
        }
    }
}
