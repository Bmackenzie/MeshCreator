using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/***
* MeshCreatorInspector
*	modifies the inspector to show controls for the Mesh Creator.
*	this script needs to be in the Editor folder of your project along
*	with the SimpleSurfaceEdge.cs and the Triangulator.cs script.
*   
***/
[CustomEditor(typeof(MeshCreatorData))]
public class MeshCreatorInspector :  Editor {
	
	private MeshCreatorData mcd;
	private const float versionNumber = 0.6f;
	private bool showColliderInfo = false;
	private bool showMeshInfo = false;
	private bool showMaterialInfo = false;
	private bool showExperimentalInfo = false;
    private bool showToolInfo = false;

    // enums for mesh and collider type
    private ObjectColliderType colliderType;
    private ObjectMeshType meshType;

	/***
	* OnEnable
	* 	set the MeshCreator when component is added to the object
	***/
	private void OnEnable()
    {
		mcd = target as MeshCreatorData;
		if (mcd == null) {
			Debug.LogError("MeshCreatorInspector::OnEnable(): couldn't find a MeshCreatorData component");
		}
    }
	 
	/***
	* OnInspectorGUI
	*	this does the main display of information in the inspector.
	***/
	public override void OnInspectorGUI() {
		EditorGUIUtility.LookLikeInspector();
		
		// TODO: inspector layout should be redesigned so that it's easier to 
		//	 see the texture and material information
		if (mcd != null) {
			EditorGUILayout.LabelField("UCLA Game Lab Mesh Creator" );
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Creation Outline", "");
			mcd.outlineTexture = 
				EditorGUILayout.ObjectField("Mesh Outline Texture", mcd.outlineTexture, typeof(Texture2D), true) as Texture2D;
            mcd.pixelTransparencyThreshold = EditorGUILayout.Slider("  Pixel Threshold", mcd.pixelTransparencyThreshold, 1.0f, 255.0f);

            EditorGUILayout.Space();
            // what type of object being created, 2d or 3d?
            meshType = (ObjectMeshType)EditorGUILayout.EnumPopup("Mesh Type", meshType);
            if (meshType == ObjectMeshType.Full3D)
            {
                mcd.uvWrapMesh = true;
            }
            else
            {
                mcd.uvWrapMesh = false;
            }

            //with colliders?
            colliderType = (ObjectColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType);
            if (colliderType == ObjectColliderType.None)
            {
                mcd.generateCollider = false;
            }
            else if (colliderType == ObjectColliderType.Mesh)
            {
                mcd.generateCollider = true;
                mcd.usePrimitiveCollider = false;
            }
            else // ObjectColliderType.Boxes
            {
                mcd.generateCollider = true;
                mcd.usePrimitiveCollider = true;
            }

			EditorGUILayout.Space();
			
			if (mcd.uvWrapMesh	) EditorGUILayout.TextArea("A 3d mesh will be created.");
			else 
			{
				if (mcd.createEdges && mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created, with a mesh side edge.");
				else if (mcd.createEdges) EditorGUILayout.TextArea("A flat front plane will be created, with a mesh side edge.");
				else if (mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created.");
				else EditorGUILayout.TextArea("A flat front plane will be created.");
			}
						
			EditorGUILayout.Space();
			showMeshInfo = EditorGUILayout.Foldout(showMeshInfo, "Mesh Creation");
			if (showMeshInfo)
			{
				EditorGUILayout.LabelField("  Mesh id number", mcd.idNumber );
                if (!mcd.uvWrapMesh) {
					mcd.createEdges = EditorGUILayout.Toggle("  Create full mesh for edge?", mcd.createEdges);
					mcd.createBacksidePlane = EditorGUILayout.Toggle("  Create backside plane?", mcd.createBacksidePlane);
				}
			}
			
			EditorGUILayout.Space();
			showMaterialInfo = EditorGUILayout.Foldout(showMaterialInfo, "Mesh Materials");
			if (showMaterialInfo)
			{
				mcd.useAutoGeneratedMaterial = EditorGUILayout.Toggle("  Auto Generate Material?", mcd.useAutoGeneratedMaterial);
				if (!mcd.useAutoGeneratedMaterial) mcd.frontMaterial = 
					EditorGUILayout.ObjectField("    Use Other Material", mcd.frontMaterial, typeof(Material), true ) as Material;
			
				
			}
			
			EditorGUILayout.Space();
			showColliderInfo = EditorGUILayout.Foldout(showColliderInfo, "Collider Creation");
			if (showColliderInfo)
			{
				if (mcd.generateCollider && mcd.usePrimitiveCollider) mcd.smallestBoxArea = EditorGUILayout.FloatField("  Smallest Box Area", mcd.smallestBoxArea);
				if (mcd.generateCollider) {
					mcd.usePhysicMaterial = EditorGUILayout.Toggle("  Use Physics Material?", mcd.usePhysicMaterial);
					if (mcd.usePhysicMaterial) mcd.physicMaterial = 
						EditorGUILayout.ObjectField("    Physical Material", mcd.physicMaterial, typeof(PhysicMaterial), true) as PhysicMaterial;
                    mcd.setTriggers = EditorGUILayout.Toggle("  Set Collider Triggers?", mcd.setTriggers);
                    mcd.addRigidBody = EditorGUILayout.Toggle("  Add Rigidbody?", mcd.addRigidBody);
                    
				}
			}
			
			EditorGUILayout.Space();
			showExperimentalInfo = EditorGUILayout.Foldout(showExperimentalInfo, "Experimental");
			if (showExperimentalInfo)
			{				
				mcd.mergeClosePoints = EditorGUILayout.Toggle( "  Merge Close Points", mcd.mergeClosePoints);
				//mcd.mergePercent = EditorGUILayout.FloatField( "Merge Percent Points", mcd.mergePercent);
				mcd.mergeDistance = EditorGUILayout.FloatField( "  Merge Distance (px)", mcd.mergeDistance);
				EditorGUILayout.Space();
				
				EditorGUILayout.LabelField("  Pivot Position", "");
				mcd.pivotHeightOffset = EditorGUILayout.FloatField("    Pivot Height Offset", mcd.pivotHeightOffset);
				mcd.pivotWidthOffset = EditorGUILayout.FloatField("    Pivot Width Offset", mcd.pivotWidthOffset);
				mcd.pivotDepthOffset = EditorGUILayout.FloatField("    Pivot Depth Offset", mcd.pivotDepthOffset);
			}
			
			EditorGUILayout.Space();
			if (GUILayout.Button("Update Mesh", GUILayout.MaxWidth(100))) {
				// do some simple parameter checking here so we don't get into trouble
				if (mcd.smallestBoxArea < 2) {
					Debug.LogWarning("Mesh Creator: smallest box area should be larger than 1.");
				}
				else {
					MeshCreator.UpdateMesh(mcd.gameObject);
					//Editor.Repaint(); // error when deleting colliders
				}
			}
            showToolInfo = EditorGUILayout.Foldout(showToolInfo, "Mesh Creator Info");
            if (showToolInfo)
            {
                
                EditorGUILayout.LabelField("  Mesh Creator Data", "version " + MeshCreatorData.versionNumber.ToString());
                EditorGUILayout.LabelField("  Mesh Creator Editor", "version " + versionNumber.ToString());
                EditorGUILayout.LabelField("  Mesh Creator", "version " + MeshCreator.versionNumber.ToString());

            }
		}
		else {
			Debug.LogError("MeshCreatorInspector::OnInspectorGUI(): couldn't find a MeshCreatorData component");
		}
		
	}
	
}

