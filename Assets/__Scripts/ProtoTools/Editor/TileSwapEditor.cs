// #define USE_TILE_SWAP_EDITOR

#if USE_TILE_SWAP_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;  // We need this to make Tiles & Tilemaps work


[CustomPropertyDrawer( typeof( TileSwap ) )]
public class TileSwapEditor : PropertyDrawer {
    private static Dictionary<string, Rect> RECT_DICT;
    private static Dictionary<int, Sprite> TILE_SPRITE_DICT;

    // Define several variables that will layout these Rects
    private const float cTotalHeight = 80;
    private const float cSpriteSize = 16 * 3;
    private const float cFromLabelWidth = 46;
    private const float cToLabelWidth = 32;
    private const float cNumWidth = 40;
    private const float cBorder = 8;
    private const float cPrefabLabelWidth = 100;


    void PrepRects( float lineHeight = 18 ) {
        RECT_DICT = new Dictionary<string, Rect>();

        // Add relative Rects for each element in the PropertyDrawer
        RECT_DICT.Add( "fromTileImg", new Rect( 0, ( lineHeight * 3 - cSpriteSize ) / 2, cSpriteSize, cSpriteSize ) );
        RECT_DICT.Add( "toTileImg", new Rect( -cSpriteSize, ( lineHeight * 3 - cSpriteSize ) / 2, cSpriteSize, cSpriteSize ) );
        RECT_DICT.Add( "fromTileLabel", new Rect( cSpriteSize + cBorder, 0, cFromLabelWidth, lineHeight ) );
        RECT_DICT.Add( "fromTileNum", new Rect( cSpriteSize + cBorder + cFromLabelWidth, 0, cNumWidth, lineHeight ) );
        RECT_DICT.Add( "toTileLabel", new Rect( -cSpriteSize - cBorder - cNumWidth - cToLabelWidth, 0, cToLabelWidth, lineHeight ) );
        RECT_DICT.Add( "toTileNum", new Rect( -cSpriteSize - cBorder - cNumWidth, 0, cNumWidth, lineHeight ) );
        RECT_DICT.Add( "swapPrefabLabel", new Rect( cSpriteSize + cBorder, lineHeight, cPrefabLabelWidth, lineHeight ) );
        RECT_DICT.Add( "swapPrefab", new Rect( cSpriteSize + cBorder + cPrefabLabelWidth, lineHeight, -( cSpriteSize + cBorder ) * 2 - cPrefabLabelWidth, lineHeight ) );
        RECT_DICT.Add( "guaranteedDropLabel", new Rect( cSpriteSize + cBorder, lineHeight * 2, cPrefabLabelWidth, lineHeight ) );
        RECT_DICT.Add( "guaranteedDrop", new Rect( cSpriteSize + cBorder + cPrefabLabelWidth, lineHeight * 2, -( cSpriteSize + cBorder ) * 2 - cPrefabLabelWidth, lineHeight ) );

        // // Add relative Rects for each element in the PropertyDrawer
        // RECT_DICT.Add( "fromTileImg", new Rect( 0, lineHeight -16, 32, 32 ) );
        // RECT_DICT.Add( "toTileImg", new Rect( -32, lineHeight -16, 32, 32 ) );
        // RECT_DICT.Add( "fromTileLabel", new Rect( 40, 0, 40, lineHeight ) );
        // RECT_DICT.Add( "fromTileNum", new Rect( 40, lineHeight, 40, lineHeight ) );
        // RECT_DICT.Add( "toTileLabel", new Rect( -70, 0, 30, lineHeight ) );
        // RECT_DICT.Add( "toTileNum", new Rect( -70, lineHeight, 30, lineHeight ) );
        // RECT_DICT.Add( "swapPrefabLabel", new Rect( 80, 0, 40, lineHeight ) );
        // RECT_DICT.Add( "swapPrefab", new Rect( 80+40, 0, -80-70-40, lineHeight ) );
        // RECT_DICT.Add( "guaranteedDropLabel", new Rect( 80, lineHeight, 64, lineHeight ) );
        // RECT_DICT.Add( "itemDropPrefab", new Rect( 80+40, lineHeight, -80-70-40, lineHeight ) );
    }

    void PrepTileSpriteDict() {
        TILE_SPRITE_DICT = new Dictionary<int, Sprite>();

        // Load all of the Sprites from DelverTiles
        Tile[] tempTiles = Resources.LoadAll<Tile>( "Tiles_Visual" );        // a

        // The order of the Tiles is not guaranteed, so arrange them by number
        int num = 0;
        for ( int i = 0; i < tempTiles.Length; i++ ) {
            string[] bits = tempTiles[i].name.Split( '_' );                  // b
            if ( int.TryParse( bits[1], out num ) ) {
                TILE_SPRITE_DICT[num] = tempTiles[i].sprite;
            } else {
                Debug.LogError( "Failed to parse num of: " + tempTiles[i].name );// c
            }
        }
        Debug.Log( "Parsed " + TILE_SPRITE_DICT.Count + " tiles into TILE_SPRITE_DICT." );
    }

    Rect RelativeRect( Rect position, Rect relRect ) {
        Rect newRect = position; // Rects are structs, so this copies the data, not a reference
        newRect.x += ( relRect.x >= 0 ) ? relRect.x : position.width + relRect.x;
        newRect.y += ( relRect.y >= 0 ) ? relRect.y : position.height + relRect.y;
        // newRect.x += relRect.x;
        // newRect.y += relRect.y;
        // if ( relRect.x >= 0 ) {
        //     newRect.x += relRect.x;
        // } else {
        //     newRect.x += position.width + relRect.x;
        // }
        // if ( relRect.y >= 0 ) {
        //     newRect.y = relRect.y;
        // } else {
        //     newRect.y = position.y + relRect.y;
        // }
        if ( relRect.width >= 0 ) {
            newRect.width = relRect.width;
        } else {
            newRect.width = position.width + relRect.width;
        }
        if ( relRect.height >= 0 ) {
            newRect.height = relRect.height;
        } else {
            newRect.height = position.height + relRect.height;
        }
        return newRect;
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        return 80;
    }

    // Draw the property inside the given rect
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty( position, label, property );

        int fromTileNum = property.FindPropertyRelative( "fromTileNum" ).intValue;
        int toTileNum = property.FindPropertyRelative( "toTileNum" ).intValue;

        // Draw label
        // position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        GUIContent label2 = new GUIContent( label );
        label2.text = "Tile " + fromTileNum + " Swap";
        EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label2 );
        float dedent = 20;
        position.x -= dedent;
        position.width += dedent;
        position.y += 18;

        // Don't make child fields be indented
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Manage all elements in the PropertyDrawer
        if ( RECT_DICT == null ) PrepRects();//position.height);
        if ( TILE_SPRITE_DICT == null ) PrepTileSpriteDict();
        foreach ( string prop in RECT_DICT.Keys ) {
            Rect relRect = RelativeRect( position, RECT_DICT[prop] );
            switch ( prop ) {
            case "fromTileLabel":
                EditorGUI.LabelField( relRect, "From #" );
                break;
            case "toTileLabel":
                EditorGUI.LabelField( relRect, "To #" );
                break;
            case "swapPrefabLabel":
                EditorGUI.LabelField( relRect, "Swap Prefab" );
                break;
            case "guaranteedDropLabel":
                EditorGUI.LabelField( relRect, "Guaranteed Drop" );
                break;

            case "fromTileImg":
            case "toTileImg":
                int num = ( prop == "fromTileImg" ) ? fromTileNum : toTileNum;
                if ( !TILE_SPRITE_DICT.ContainsKey( num ) ) continue;
                DrawSpritePreview( relRect, TILE_SPRITE_DICT[num] );
                // EditorGUI.DrawPreviewTexture( relRect, 
                //     TILE_SPRITE_DICT[num] );
                break;
            default:
                EditorGUI.PropertyField( relRect,
                    property.FindPropertyRelative( prop ),
                    GUIContent.none );

                break;
            }
        }

        // // Calculate rects
        // Rect rFromTile = new Rect()
        // Rect rTileNum = new Rect(position.x, position.y, 30, position.height);
        // Rect rSwapPrefab = new Rect(position.x + 35, position.y, 50, position.height);
        // Rect rItemDrop = new Rect(position.x + 90, position.y, position.width - 90, position.height);
        //
        // // Draw fields - passs GUIContent.none to each so they are drawn without labels
        // EditorGUI.PropertyField(rTileNum, property.FindPropertyRelative("tileNum"), GUIContent.none);
        // EditorGUI.PropertyField(rSwapPrefab, property.FindPropertyRelative("swapPrefab"), GUIContent.none);
        // EditorGUI.PropertyField(rItemDrop, property.FindPropertyRelative("guaranteedDrop"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    // Created by WoofyWyzpets https://forum.unity.com/threads/drawing-a-sprite-in-editor-window.419199/
    private void DrawSpritePreview( Rect position, Sprite sprite ) {
        Vector2 fullSize = new Vector2( sprite.texture.width, sprite.texture.height );
        Vector2 size = new Vector2( sprite.textureRect.width, sprite.textureRect.height );

        Rect coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min( ratio.x, ratio.y );

        Vector2 center = position.center;
        position.width = size.x * minRatio;
        position.height = size.y * minRatio;
        position.center = center;

        GUI.DrawTextureWithTexCoords( position, sprite.texture, coords );
    }
}

// [CustomEditor( typeof(TileSwap) )]
// public class TileSwapEditor : Editor {
//     SerializedProperty tileNum;
//     SerializedProperty swapPrefab;
//     SerializedProperty guaranteedDrop;
//     SerializedProperty overrideTileNum;
//
//     void OnEnable() {
//         tileNum = serializedObject.FindProperty( "tileNum" );
//         swapPrefab = serializedObject.FindProperty( "swapPrefab" );
//         guaranteedDrop = serializedObject.FindProperty( "guaranteedDrop" );
//         overrideTileNum = serializedObject.FindProperty( "overrideTileNum" );
//     }
//
//     public override void OnInspectorGUI() {
//         serializedObject.Update();
//         EditorGUILayout.PropertyField( tileNum );
//         EditorGUILayout.PropertyField( swapPrefab );
//         EditorGUILayout.PropertyField( guaranteedDrop );
//         EditorGUILayout.PropertyField( overrideTileNum );
//         serializedObject.ApplyModifiedProperties();
//     }
// }

#endif