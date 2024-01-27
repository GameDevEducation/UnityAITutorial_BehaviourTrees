using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] EFaction _Faction;

    public EFaction Faction => _Faction;
}
