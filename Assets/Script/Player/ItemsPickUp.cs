using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            Items items = collision.GetComponent<Items>();
            if (items != null)
            {
                ItemsDetails itemsDetails = InventoryManager.Instance.GetItemsDetails(items.Items_code);
                if(itemsDetails.canBePickUp == true)
                {
                    InventoryManager.Instance.AddItem(InventoryLocation.player, items, collision.gameObject);
                    //Debug.Log("pengzhuangfasheng");
                }
            }
        }
    }
}
