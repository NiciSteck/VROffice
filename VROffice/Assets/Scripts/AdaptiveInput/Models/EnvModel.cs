using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvModel : MonoBehaviour , IComparable<EnvModel>
{
    [NonSerialized]
    public int similarity = 0;
    
    [SerializeField]
    public List<ContainerModel> containers = new List<ContainerModel>();
    
    public int CompareTo( EnvModel that ) {
        if ( that == null ) return 1;
        if ( this.similarity > that.similarity) return -1;
        if ( this.similarity < that.similarity) return 1;
        return 0;
    }
    
}
