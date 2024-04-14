using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageComponent : MonoBehaviour {

    public GameObject hitFX;
	void DealDamage() {
        if (transform.parent == null)
            return;
        DemoController dc = transform.parent.GetComponent<DemoController>();
        if (dc != null)
            dc.DealDamage(this);
    }
	

}
