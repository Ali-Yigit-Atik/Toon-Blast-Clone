using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ArrayLayout))]
public class CustPropertyDrawer : PropertyDrawer {

	private int row_ = GameObject.FindGameObjectWithTag("matchMaster").gameObject.GetComponent<MatchAndBlast>().column;
	private int column = GameObject.FindGameObjectWithTag("matchMaster").gameObject.GetComponent<MatchAndBlast>().row;
	public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		EditorGUI.PrefixLabel(position,label);
		Rect newposition = position;
		newposition.y += 18f;
		SerializedProperty data = property.FindPropertyRelative("rows");
        if (data.arraySize != column)
            data.arraySize = column;
		//data.rows[0][]
		for(int j=0;j<column;j++){
			SerializedProperty row = data.GetArrayElementAtIndex(j).FindPropertyRelative("row");
			newposition.height = 18f;
			if(row.arraySize != row_) //
				row.arraySize = row_; //
			newposition.width = position.width/row_; //
			for(int i=0;i<row_;i++)
			{ //
				EditorGUI.PropertyField(newposition,row.GetArrayElementAtIndex(i),GUIContent.none);
				newposition.x += newposition.width;
			}

			newposition.x = position.x;
			newposition.y += 18f;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
		return 18f * 15;
	}
}
