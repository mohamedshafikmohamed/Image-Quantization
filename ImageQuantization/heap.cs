using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{

    public class heap
    {
        public List<Node> elements = new List<Node>();
        public heap()
        {
        }
        //Extracts minimum from the queue
        public Node extract_Min()
        {
            if (elements.Count > 0)//O(1)
            {
                Node min = elements[0];//O(1)
                elements[0] = elements[elements.Count - 1];//O(1)
                elements.RemoveAt(elements.Count - 1);//O(1)
                min_Heapify(0);//O(Log(N))
                return min;//O(1)
            }
            //If the queue is empty, shows an exception message
            throw new InvalidOperationException("No elements in the heap");//O(1)
        }
        // total extract_Min() complexity --> O(Log(N)) --> N = number of elements in heap

        //Gets the element that is over the current index
        public int get_Parent(int index)
        {
            if (index <= 0)
            {
                return -1;
            }
            return (index - 1) / 2;
        }
        //Gets the element on the bottom left of the current index
        public int get_Left(int index)
        {
            return 2 * index + 1;
        }
        //Gets the element on the bottom right of the current index
        public int get_Right(int index)
        {
            return 2 * index + 2;
        }
        //Compares the element of the current index with the left bottom and right bottom child and swaps with the smallest element if it exists
        public void min_Heapify(int index)
        {
            int smallest = index;//O(1)
            int left = get_Left(index);//O(1)
            int right = get_Right(index);//O(1)
            if (left < elements.Count && elements[left].weight < elements[smallest].weight)//O(1)
            {
                smallest = left;//O(1)

            }
            if (right < elements.Count && elements[right].weight < elements[smallest].weight)//O(1)
            {
                smallest = right;//O(1)
            }
            if (smallest != index)//O(1)
            {
                Node temp = elements[index];//O(1)
                elements[index] = elements[smallest];//O(1)
                elements[smallest] = temp;//O(1)
                min_Heapify(smallest);
            }
        }
        // total min_Heapify() complexity --> O(Log(N)) --> N = number of elements in heap

        //Inserts an element to a new leaf = element.Count -1, and then uses heapify_Up function to check whether the element that is on top is bigger than the current element or not
        //If the current element is less than the one above it then it swaps the two elements and recurse until there is no element above it that is less than it
        public void insert(Node element)
        {
            elements.Add(element);//O(1)
            heapify_Up(elements.Count - 1);//O(Log(N))
        }
        // total insert() complexity --> O(Log(N)) --> N = number of elements in heap

        //Inserts an element to a specified index by making the element of this index equals to negative infinity
        //And then calls several other function to determine the new order of the priority queue
        public void insert_Key(int index, Node n_element)
        {
            double dd = double.MinValue;
            elements[index].set_key(dd);
            heapify_Up(index);
            min_Heapify(index);
            extract_Min();
            insert(n_element);
        }
        public void remove(int index)
        {
            double dd = double.MinValue;
            elements[index].set_key(dd);
            heapify_Up(index);
            min_Heapify(index);
            extract_Min();
        }
        //Compares the current element with it's parent if the parent is bigger than current element than a swap is made 
        public void heapify_Up(int index)
        {
            int parent = get_Parent(index);//O(1)
            if (parent >= 0 && elements[parent].weight > elements[index].weight)//O(1)
            {
                Node temp = elements[index];//O(1)
                elements[index] = elements[parent];//O(1)
                elements[parent] = temp;//O(1)
                heapify_Up(parent);
            }
        }
        // total heapify_Up complexity --> O(Log(N)) --> N = number of elements in heap

        //Checks whether the priority queue is empty or not
        public bool empty()
        {
            if (elements.Count > 0)
                return false;
            else
                return true;
        }
    }
}
