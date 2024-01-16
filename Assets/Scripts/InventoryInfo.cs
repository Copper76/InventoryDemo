using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
public struct ItemInfo
{
    public int item_id;
    public int stack;
    public GameObject item;

    public ItemInfo(GameObject item)
    {
        this.item = item;
        item_id = 0;
        stack = 0;
    }
}
**/
public class InventoryInfo : MonoBehaviour
{
    //public ItemInfo[] items;
    public GameObject[] items;
    public int[] item_id;
    public int[] stack;

    public int[] maxStackDict;//an array representing stack limit for item based on id
    public int[] equipSlot;

    public GameObject[] pickUpPrefabs; 

    private void Awake()
    {
        maxStackDict = new int[] { 5,5,5,5,5,5,5,5,5};
        equipSlot = new int[] { -1,0,1,-1,-1,-1,-1,-1,-1};
    }

    //Add to empty slot
    public void AddItem(Sprite item, int id, int index)
    {
        items[index].GetComponent<Image>().sprite = item;
        item_id[index] = id;
        stack[index] = 1;
        TextMeshProUGUI text = items[index].GetComponentInChildren<TextMeshProUGUI>();
        text.enabled = true;
        text.text = stack[index].ToString();
    }

    public void AddItem(Sprite item, int id, int index, int count)
    {
        items[index].GetComponent<Image>().sprite = item;
        item_id[index] = id;
        stack[index] = count;
        TextMeshProUGUI text = items[index].GetComponentInChildren<TextMeshProUGUI>();
        text.enabled = true;
        text.text = stack[index].ToString();
    }

    public void StackItem(int index)
    {
        stack[index] += 1;
        TextMeshProUGUI text = items[index].GetComponentInChildren<TextMeshProUGUI>();
        text.text = stack[index].ToString();
    }

    public void StackItem(int index, int count)
    {
        stack[index] += count;
        TextMeshProUGUI text = items[index].GetComponentInChildren<TextMeshProUGUI>();
        text.text = stack[index].ToString();
    }

    public int FindStackSlot(int id)
    {
        int target_index;
        int start_index = 0;
        do
        {
            target_index = Array.IndexOf(item_id, id, start_index);
            start_index = target_index + 1;
        } while (target_index != -1 && stack[target_index] >= maxStackDict[id] && start_index < item_id.Length);
        return target_index;
    }

    public int FindAvailableSlot()
    {
        for (int i = 0; i < stack.Length; i++)
        {
            if (item_id[i] == 0)
            {
                return i;
            }
        }
        return -1;
    }
}
