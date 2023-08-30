using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using Redirection;
public class Logger : MonoBehaviour
{

    [HideInInspector]
    public RDManager redirectionManager;


    /*string RESULT_DIRECTORY = "Experiment Results/";
    string SUMMARY_STATISTICS_DIRECTORY = "Summary Statistics/";
    string SAMPLED_METRICS_DIRECTORY = "Sampled Metrics/";

    XmlWriter xmlWriter;
    public string SUMMARY_STATISTICS_XML_FILENAME = "SimulationResults";
    const string XML_ROOT = "Experiments";
    const string XML_ELEMENT = "Experiment";

    StreamWriter csvWriter;

    void Awake()
    {
        RESULT_DIRECTORY = SnapshotGenerator.GetProjectPath() + RESULT_DIRECTORY;
        SUMMARY_STATISTICS_DIRECTORY = RESULT_DIRECTORY + SUMMARY_STATISTICS_DIRECTORY;
        SAMPLED_METRICS_DIRECTORY = RESULT_DIRECTORY + SAMPLED_METRICS_DIRECTORY;
        SnapshotGenerator.CreateDirectoryIfNeeded(RESULT_DIRECTORY);
        SnapshotGenerator.CreateDirectoryIfNeeded(SUMMARY_STATISTICS_DIRECTORY);
        SnapshotGenerator.CreateDirectoryIfNeeded(SAMPLED_METRICS_DIRECTORY);
        SnapshotGenerator.CreateDirectoryIfNeeded(SnapshotGenerator.DEFAULT_SNAPSHOT_DIRECTORY);
    }*/

    private void Start()
    {
        redirectionManager = GetComponent<RDManager>();

        //Get the path of the Game data folder
        //string m_Path = Application.dataPath;

        //Output the Game data path to the console
        //redirectionManager.text1.SetText(m_Path);

    }

   /* public void LogOneDimensionalExperimentSamples(string experimentDecriptorString, string measuredMetric, List<float> values)
    {
        string experimentSamplesDirectory = SAMPLED_METRICS_DIRECTORY + experimentDecriptorString + "/";
        CreateDirectoryIfNeeded(experimentSamplesDirectory);
        csvWriter = new StreamWriter(experimentSamplesDirectory + measuredMetric + ".csv");
        foreach (float value in values)
        {
            csvWriter.WriteLine(value);
        }
        csvWriter.Flush();
        csvWriter.Close();
    }

    public void LogTwoDimensionalExperimentSamples(string experimentDecriptorString, string measuredMetric, List<Vector2> values)
    {
        //csvWriter = new StreamWriter(measuredMetric + "_" + experimentDecriptorString + ".csv");
        string experimentSamplesDirectory = SAMPLED_METRICS_DIRECTORY + experimentDecriptorString + "/";
        CreateDirectoryIfNeeded(experimentSamplesDirectory);
        csvWriter = new StreamWriter(experimentSamplesDirectory + measuredMetric + ".csv");
        foreach (Vector2 value in values)
        {
            csvWriter.WriteLine(value.x + ", " + value.y);
        }
        csvWriter.Flush();
        csvWriter.Close();
    }

    public static void CreateDirectoryIfNeeded(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }*/
}
