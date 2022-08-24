using System;

public class Test
{
    public void Main (object[] args)
    {
        Console.WriteLine(Solution(new int[] { 1, 5, 4, 2, 1, 6 }));
    }

    public int Solution (int[] A)
    {
        Array.Sort(A);
        int testElement = 1;
        for (int i = 0; i < A.Length; i++)
        {
            if (A[i] < testElement)
                continue;
            if (A[i] == testElement)
            {
                testElement++;
                continue;
            }
            if (A[i] > testElement)
                break;
        }
        return testElement;
    }
}
