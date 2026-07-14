// Copyright (c) Jason Ma
// LWGUI - Amplify Shader Editor Property Node Extension
// Bridges LWGUI drawer/decorator system into ASE's IPropertyNodeExtension API.
// Requires both LWGUI and AmplifyShaderEditor assemblies to be present.

#if LWGUI_ASE_INTEGRATION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AmplifyShaderEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Sets ASE's default custom inspector to LWGUI when this bridge is active.
	/// Runs once at domain reload. When the integration is disabled (symbol removed),
	/// this code doesn't compile, so PreferredCustomInspector stays null (ASE default).
	/// </summary>
	internal static class LwguiCustomInspectorSetup
	{
		[InitializeOnLoadMethod]
		static void SetPreferredCustomInspector()
		{
			Constants.PreferredCustomInspector = "LWGUI.LWGUI";
		}
	}

	public sealed class LwguiPropertyNodeExtension : IPropertyNodeExtension
	{
		private const string Id = "LWGUI";

		#region Per-Node Data Model

		[Serializable]
		internal class NodeData
		{
			public LwguiAttributeData drawer = new LwguiAttributeData();
			public List<LwguiAttributeData> decorators = new List<LwguiAttributeData>();

			[NonSerialized] public bool drawerFoldout = true;
			[NonSerialized] public bool decoratorsFoldout = true;
			[NonSerialized] public ReorderableList decoratorsReorderableList;
		}

		private static readonly ConditionalWeakTable<PropertyNode, NodeData> s_data =
			new ConditionalWeakTable<PropertyNode, NodeData>();

		#endregion

		#region IPropertyNodeExtension

		public string ExtensionId => Id;

		public void DrawUI( PropertyNode node )
		{
			var data = GetData( node );
			var onChanged = new Action( () =>
			{
				node.IsDirty = true;
				node.SetSaveIsDirty();
			} );

			LwguiExtensionHelper.DrawLwguiDrawerSection(
				data.drawer, ref data.drawerFoldout, onChanged, node );

			LwguiExtensionHelper.DrawLwguiDecoratorsSection(
				data.decorators, ref data.decoratorsFoldout, onChanged, node,
				ref data.decoratorsReorderableList );
		}

		public string BuildAttributes( PropertyNode node )
		{
			var data = GetData( node );
			return LwguiExtensionHelper.GenerateAttributesString( data.drawer, data.decorators );
		}

		public string WriteData( PropertyNode node )
		{
			var data = GetData( node );
			bool hasDrawer = data.drawer != null && !string.IsNullOrEmpty( data.drawer.drawerTypeName );
			bool hasDecorators = data.decorators != null && data.decorators.Any(
				d => !string.IsNullOrEmpty( d.drawerTypeName ) );

			if ( !hasDrawer && !hasDecorators )
				return string.Empty;

			return Convert.ToBase64String(
				Encoding.UTF8.GetBytes( JsonUtility.ToJson( data ) ) );
		}

		public void ReadData( PropertyNode node, string token )
		{
			if ( string.IsNullOrEmpty( token ) ) return;

			try
			{
				var data = GetData( node );
				JsonUtility.FromJsonOverwrite(
					Encoding.UTF8.GetString( Convert.FromBase64String( token ) ), data );
			}
			catch ( Exception e )
			{
				Debug.LogWarning(
					$"[LWGUI] Failed to parse ASE extension token for node '{node?.GetType().Name ?? "unknown"}': {e.Message}" );
			}
		}

		#endregion

		#region Data Access

		/// <summary>
		/// Gets the per-node data, restoring from persisted token on cache miss (e.g. after domain reload).
		/// </summary>
		internal static NodeData GetData( PropertyNode node )
		{
			if ( !s_data.TryGetValue( node, out NodeData data ) )
			{
				data = new NodeData();
				DecodeInto( data, PropertyNodeExtensions.GetToken( node, Id ) );
				s_data.Add( node, data );
			}
			return data;
		}

		private static void DecodeInto( NodeData data, string token )
		{
			if ( string.IsNullOrEmpty( token ) ) return;
			try
			{
				JsonUtility.FromJsonOverwrite(
					Encoding.UTF8.GetString( Convert.FromBase64String( token ) ), data );
			}
			catch ( Exception e )
			{
				Debug.LogWarning( $"[LWGUI] Failed to decode ASE extension token: {e.Message}" );
			}
		}

		#endregion
	}
}
#endif
