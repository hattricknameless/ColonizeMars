using System;
using System.Collections.Generic;
using UnityEngine;

//Spent too much time on figuring out what should I use to store my wfc data

[Serializable] //Temporary class used for structure json rules
public class InputRule { 
    public string rule;
    public string tileType;
    public string rotation;
    public string px;
    public string nx;
    public string py;
    public string ny;
    public string pz;
    public string nz;
}

[Serializable] //Temporary class used for import json files
public class InputRulebook {
    public List<InputRule> rules;
}

[Serializable] //Final output of the SocketRules
public class SocketRule {
    public string ruleName;
    public TileType tileType;
    public int rotation;
    public SocketType px;
    public SocketType nx;
    public SocketType py;
    public SocketType ny;
    public SocketType pz;
    public SocketType nz;

    //Convert from InputRule to SocketRule
    public static SocketRule ConvertFromInput(InputRule input) {
        SocketRule output = new();
        output.ruleName = input.rule;
        
        if (Enum.TryParse(input.tileType, out TileType tileType)) {
            output.tileType = tileType;
        }
        else {
            Debug.LogWarning($"TileType parse error, rule: {output.ruleName}/{input.tileType}");
        }

        output.rotation = int.Parse(input.rotation);

        output.px = (SocketType)Enum.Parse(typeof(SocketType), input.px);
        output.py = (SocketType)Enum.Parse(typeof(SocketType), input.py);
        output.pz = (SocketType)Enum.Parse(typeof(SocketType), input.pz);
        output.nx = (SocketType)Enum.Parse(typeof(SocketType), input.nx);
        output.ny = (SocketType)Enum.Parse(typeof(SocketType), input.ny);
        output.nz = (SocketType)Enum.Parse(typeof(SocketType), input.nz);

        return output;
    }

    public static SocketRule UndefinedRule() {
        SocketRule output = new();
        
        output.ruleName = "Undefined";
        output.tileType = TileType.Undefined;
        
        return output;
    }
}

public enum TileType {
    Bot, Bot_Corner, Bot_Inverse,
    Top, Top_Corner, Top_Inverse,
    Mid, Mid_Corner, Mid_Inverse,
    Full, Empty, Slope,
    Undefined
}

public enum SocketType {
    Top_Side_L, Top_Side_R,
    Mid_Side_L, Mid_Side_R,
    Bot_Side_L, Bot_Side_R,
    Corner_Vertical_0, Corner_Vertical_1, Corner_Vertical_2, Corner_Vertical_3,
    Inverse_Vertical_0, Inverse_Vertical_1, Inverse_Vertical_2, Inverse_Vertical_3,
    Edge_Vertical_0, Edge_Vertical_1, Edge_Vertical_2, Edge_Vertical_3,
    Full_Vertical, Full_Side, Empty,
    Undefined
}