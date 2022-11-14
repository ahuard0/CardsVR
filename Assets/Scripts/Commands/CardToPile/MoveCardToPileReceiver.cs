using CardsVR.Interaction;
using CardsVR.States;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   MoveCardToPileReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   MoveCardToPileReceiver is called by the Execute method of the MoveCardToPile class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class MoveCardToPileReceiver
    {
        /*
         *      Moves a card GameObject according to the instructions contained within the CardToPile command.
         *      
         *      Parameters
         *      ----------
         *      command : CardToPile
         *          A command object representing the movement of a card GameObject to a pile.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(CardToPile command)
        {
            string name_str = command.Name;
            int pile_num = command.Pile;
            int cardID = command.CardID;

            GameObject card = GameManager.Instance.getCardByTag(cardID);

            if (card == null)
                Debug.LogErrorFormat("Card not found: {0}", name_str);

            GameObject pileAnchor = GameManager.Instance.getPileCardAnchor(pile_num);

            if (pileAnchor == null)
                Debug.LogErrorFormat("Pile Num ({0}) Anchor not found.", pile_num);

            CardStateContext context = card.GetComponent<CardStateContext>();

            if (context == null)
                Debug.LogError("Could not find state context.");

            // Update GameObject
            card.transform.parent = pileAnchor.transform;  // attach card to the pile anchor
            context.PileNum = pile_num;
            context.ChangeState(context.pileState);

            int? pile_src = GameManager.Instance.getPileNumByCard(cardID);
            if (pile_src == null)  // Card not in any pile stack
                GameManager.Instance.getPileCardStack(pile_num).Push(cardID);  // Simply add the card to the stack.  No need to remove it from anywhere first.
            else
            {
                // Remove the CardID element from the source GameManager stack
                Stack<int> temp = new Stack<int>();
                Stack<int> stack_src = GameManager.Instance.getPileCardStack((int)pile_src);
                int stack_count = stack_src.Count;
                for (int j = 0; j < stack_count; j++)
                {
                    temp.Push(stack_src.Pop());  // reverse operation on stack
                }
                int temp_count = temp.Count;
                for (int j = 0; j < temp_count; j++)
                {
                    if (temp.Peek() != cardID)  // test for cardID
                        stack_src.Push(temp.Pop());
                    else
                        temp.Pop();  // discard matches
                }
                temp.Clear();

                // Add the CardID to the destination GameManager stack
                GameManager.Instance.getPileCardStack(pile_num).Push(cardID);
            }
        }
    }
}
