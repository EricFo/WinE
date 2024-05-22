using System;
using SlotGame.Core;
using UnityEngine;
using UniversalModule.SpawnSystem;

public class PayLine : SpawnItem
{
    private LineRenderer line;
    private LineRenderer Line {
        get {
            if (line == null){
                line = GetComponent<LineRenderer>();
            }
            return line;
        }
    }

    /// <summary>
    /// 绘制线段
    /// </summary>
    /// <param name="Pos"></param>
    public void DrawLine(Vector3[] Pos) {
        Vector3[] linePoint = new Vector3[Pos.Length + 2];
        Array.Copy(Pos, 0, linePoint, 1, Pos.Length);
        if (GlobalObserver.CurrGameState==GameState.Free&& GlobalObserver.FreeGamePlay.Contains(1))
        {
            linePoint[0] = Pos[0] + (Vector3.left * 0.8f);
            linePoint[linePoint.Length - 1] = Pos[Pos.Length - 1] + (Vector3.right * 0.8f);
        }
        else
        {
            linePoint[0] = Pos[0] + (Vector3.left * 1.6f);
            linePoint[linePoint.Length - 1] = Pos[Pos.Length - 1] + (Vector3.right * 1.6f);
        }
        for (int i = 0; i < linePoint.Length; i++){
            linePoint[i].z = 0;
        }
        Line.positionCount = linePoint.Length;
        Line.SetPositions(linePoint);
        Show();
    }
    /// <summary>
    /// 显示
    /// </summary>
    public void Show() {
        Line.material.color = new Color(1, 1, 1, 1);
    }
    /// <summary>
    /// 隐藏
    /// </summary>
    public void Hide() {
        Line.material.color = new Color(1, 1, 1, 0);
    }

    public void SetLineWidth(float width)
    {
        Line.SetWidth(width,width);
    }

    public override void Recycle()
    {
        Line.SetWidth(0.27f,0.27f);
        base.Recycle();
    }
}
