using ChessLogic;
using Managers;
using UnityEngine;

public class TeamSelector : MonoBehaviour
{
    public GameManager gameManager;
    public Shared.TeamType team;
    private static readonly int FresnelPower = Shader.PropertyToID("_Fresnel_Power");

    private const float BaseFresnelPower = 6.0f;
    private const float HoveredFresnelPower = 2.5f;
    
    public void HoverInteraction(bool enter)
    {
        var material = GetComponent<MeshRenderer>().material;
        material.SetFloat(FresnelPower, enter ? HoveredFresnelPower : BaseFresnelPower);
    }

    public void SelectTeam()
    {
        gameManager.SelectTeam(team, transform.position);
    }
}
