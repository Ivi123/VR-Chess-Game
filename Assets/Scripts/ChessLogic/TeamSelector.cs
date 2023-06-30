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

    public TeamSelector whiteTeamSelector;
    public TeamSelector blackTeamSelector;
    
    public void HoverInteraction(bool enter)
    {
        var material = GetComponent<MeshRenderer>().material;
        material.SetFloat(FresnelPower, enter ? HoveredFresnelPower : BaseFresnelPower);
    }

    public void SelectTeam()
    {
        gameManager.SelectTeam(team, transform.position, Shared.ChessboardConfig.Normal);
    }
    
    public void SelectTeamForVictory()
    {
        gameManager.SelectTeam(team, whiteTeamSelector.gameObject.transform.position, Shared.ChessboardConfig.Victory);
    }
    
    public void SelectTeamForDefeat()
    {
        gameManager.SelectTeam(Shared.TeamType.Black, blackTeamSelector.gameObject.transform.position, Shared.ChessboardConfig.Defeat);
    }
    
    public void SelectTeamForDraw()
    {
        gameManager.SelectTeam(team, whiteTeamSelector.gameObject.transform.position, Shared.ChessboardConfig.Draw);
    }
    
    public void SelectTeamForProm()
    {
        gameManager.SelectTeam(team, whiteTeamSelector.gameObject.transform.position, Shared.ChessboardConfig.Promotion);
    }
    
    public void SelectTeamForLongCastle()
    {
        gameManager.SelectTeam(team, whiteTeamSelector.gameObject.transform.position, Shared.ChessboardConfig.LongCastle);
    }
}
