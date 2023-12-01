using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptOption : MonoBehaviour
{
 public HashSet<string> favor = new HashSet<string>();
    public HashSet<string> unfavor = new HashSet<string>();

    public void AddFavor(string topic)
    {
        if (unfavor.Contains(topic))
        {
            throw new InvalidOperationException($"Topic {topic} cannot be favored as it has been unfavorably selected.");
        }

        favor.Add(topic);
    }

    public void AddUnfavor(string topic)
    {
        if (favor.Contains(topic))
        {
            throw new InvalidOperationException($"Topic {topic} cannot be unfavorably selected as it has been favored.");
        }

        unfavor.Add(topic);
    }
}