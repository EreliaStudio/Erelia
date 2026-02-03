using System;

[Serializable]
public class CreatureData
{
    public int SpeciesId;
    public string Nickname;

    public event Action Changed;

    public CreatureData()
    {
    }

    public CreatureData(int speciesId, string nickname)
    {
        SpeciesId = speciesId;
        Nickname = nickname;
    }

    public void SetSpeciesId(int speciesId)
    {
        if (SpeciesId == speciesId)
        {
            return;
        }

        SpeciesId = speciesId;
        Changed?.Invoke();
    }

    public void SetNickname(string nickname)
    {
        if (Nickname == nickname)
        {
            return;
        }

        Nickname = nickname;
        Changed?.Invoke();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
