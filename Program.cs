const int THREADS_NUMBER = 4; // количество потоков
const int N = 100000; // размер массива
object locker = new object();

Random rand = new Random();
int[] resSerial = new int[N].Select(r => rand.Next(0, 5)).ToArray();  // Генереруем массив для последовательной сортировки

int[] resParallel = new int[N];
Array.Copy(resSerial, resParallel, N); // Копируем массив для паралельной сортировки

CountingSortSeriall(resSerial);
PreparallelCountingSort(resParallel);
Console.WriteLine(EqualityArray(resSerial, resParallel));


void PreparallelCountingSort(int[] inputArray) // Готовим потоки для сортировки
{
    int max = inputArray.Max();
    int min = inputArray.Min();

    int offset = -min; // Учитываем смещение 
    int[] counters = new int[max + offset + 1];

    int eachThreadCalc = N / THREADS_NUMBER; // Делим диапазоны для потоков
    var threadsParall = new List<Thread>(); // Создаем список потоков

    for (int i = 0; i < THREADS_NUMBER; i++) //Запускаем потоки
    {
        int startPos = i * eachThreadCalc;
        int endPos = (i + 1) * eachThreadCalc;
        if (i == THREADS_NUMBER -1) 
        {
            endPos = N;
        }
        threadsParall.Add(new Thread(() => CountingSortParallel(inputArray, counters, offset, startPos, endPos)));
        threadsParall[i].Start();
    }
    foreach (var thread in threadsParall)
    {
        thread.Join(); // Ожидаем поток
    }

    int index = 0;
    for (int i = 0; i < counters.Length; i++) // Запоняем массив
    {
        for (int j = 0; j < counters[i]; j++)
        {
            inputArray[index] = i - offset;
            index++;
        }
    }
}    

void CountingSortParallel(int[] inputArray, int [] counters, int offset, int startPos, int endPos)
{
    for (int i = startPos; i < endPos; i++)
    {
       lock (locker)
       {
            counters[inputArray[i] + offset]++;
       }
    }
}

void CountingSortSeriall(int[] inputArray)
{
    int max = inputArray.Max();
    int min = inputArray.Min();

    int offset = -min; // Учитываем смещение 
    int[] counters = new int[max + offset + 1];

    for (int i = 0; i < inputArray.Length; i++)
    {
        counters[inputArray[i] + offset]++; // Считаем элементы 
    }
    
    int index = 0;
    for (int i = 0; i < counters.Length; i++) // Запоняем массив
    {
        for (int j = 0; j < counters[i]; j++)
        {
            inputArray[index] = i - offset;
            index++;
        }
    }
}

bool EqualityArray(int[] firstArray, int[] secondArray) // Сравнитваем массивы
{
    for (int i = 0; i < N; i++)
    {
        if (firstArray[i] != secondArray[i])
        {
            return false;
        }
    }
    return true;
}