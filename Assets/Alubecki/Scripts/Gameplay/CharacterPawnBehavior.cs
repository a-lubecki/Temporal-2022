using UnityEngine;


public class CharacterPawnBehavior : MonoBehaviour {


    public DataCharacterInChapter dataCharacterInChapter;


    void OnDrawGizmos() {

        Gizmos.color = Color.green;

        Gizmos.DrawSphere(transform.position + 0.3f * Vector3.up, 0.4f);
        Gizmos.DrawSphere(transform.position + 0.7f * Vector3.up, 0.3f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(new Vector3(0, 0.8f, 0.3f), new Vector3(0.1f, 0.1f, 0.2f));
    }

    public CharacterBehavior InstantiateCharacterGameObject() {

        var go = GameObject.Instantiate(dataCharacterInChapter.DataCharacter.CharacterPrefab);
        var tr = go.transform;
        tr.SetParent(transform.parent);
        tr.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        tr.localPosition = transform.localPosition;
        tr.localRotation = transform.localRotation;

        var character = go.GetComponent<CharacterBehavior>();
        character.InitWithCharacterData(dataCharacterInChapter, GetComponentInChildren<BaseAIDecider>());

        //set the pawn on the character in case some objects use the pawn location to trigger things on the level prefab
        transform.SetParent(character.transform);

        return character;
    }

}
