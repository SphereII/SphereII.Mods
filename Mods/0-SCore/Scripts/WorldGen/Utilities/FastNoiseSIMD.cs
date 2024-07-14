using UnityEngine;
using POINTER = System.IntPtr;

public class FastNoiseSIMD
{
    public enum NoiseType
    {
        Value,
        ValueFractal,
        Perlin,
        PerlinFractal,
        Simplex,
        SimplexFractal,
        WhiteNoise,
        Cellular,
        Cubic,
        CubicFractal
    }

    public enum FractalType
    {
        FBM,
        Billow,
        RigidMulti
    }

    public enum CellularDistanceFunction
    {
        Euclidean,
        Manhattan,
        Natural
    }

    public enum CellularReturnType
    {
        CellValue,
        Distance,
        Distance2,
        Distance2Add,
        Distance2Sub,
        Distance2Mul,
        Distance2Div,
        NoiseLookup,
        Distance2Cave
    }

    public enum PerturbType
    {
        None,
        Gradient,
        GradientFractal,
        Normalise,
        Gradient_Normalise,
        GradientFractal_Normalise
    }

    private readonly POINTER nativePointer;

    public FastNoiseSIMD(int seed = 1337)
    {
        nativePointer = NewFastNoiseSIMD(seed);
    }

    ~FastNoiseSIMD()
    {
        NativeFree(nativePointer);
    }

    /// <summary>
    ///     Returns seed used for all noise types
    /// </summary>
    /// <returns></returns>
    public int GetSeed()
    {
        return NativeGetSeed(nativePointer);
    }

    /// <summary>
    ///     Sets seed used for all noise types -
    ///     Default: 1337
    /// </summary>
    /// <param name="seed"></param>
    public void SetSeed(int seed)
    {
        NativeSetSeed(nativePointer, seed);
    }

    /// <summary>
    ///     Sets frequency for all noise types -
    ///     Default: 0.01
    /// </summary>
    /// <param name="frequency"></param>
    public void SetFrequency(float frequency)
    {
        NativeSetFrequency(nativePointer, frequency);
    }

    /// <summary>
    ///     Sets noise return type of (Get/Fill)NoiseSet() -
    ///     Default: Simplex
    /// </summary>
    /// <param name="noiseType"></param>
    public void SetNoiseType(NoiseType noiseType)
    {
        NativeSetNoiseType(nativePointer, (int)noiseType);
    }

    /// <summary>
    ///     Sets scaling factor for individual axis -
    ///     Defaults: 1.0
    /// </summary>
    /// <param name="xScale"></param>
    /// <param name="yScale"></param>
    /// <param name="zScale"></param>
    public void SetAxisScales(float xScale, float yScale, float zScale)
    {
        NativeSetAxisScales(nativePointer, xScale, yScale, zScale);
    }


    /// <summary>
    ///     Sets octave count for all fractal noise types -
    ///     Default: 3
    /// </summary>
    /// <param name="octaves"></param>
    public void SetFractalOctaves(int octaves)
    {
        NativeSetFractalOctaves(nativePointer, octaves);
    }

    /// <summary>
    ///     Sets octave lacunarity for all fractal noise types -
    ///     Default: 2.0
    /// </summary>
    /// <param name="lacunarity"></param>
    public void SetFractalLacunarity(float lacunarity)
    {
        NativeSetFractalLacunarity(nativePointer, lacunarity);
    }

    /// <summary>
    ///     Sets octave gain for all fractal noise types -
    ///     Default: 0.5
    /// </summary>
    /// <param name="gain"></param>
    public void SetFractalGain(float gain)
    {
        NativeSetFractalGain(nativePointer, gain);
    }

    /// <summary>
    ///     Sets method for combining octaves in all fractal noise types -
    ///     Default: FBM
    /// </summary>
    /// <param name="fractalType"></param>
    public void SetFractalType(FractalType fractalType)
    {
        NativeSetFractalType(nativePointer, (int)fractalType);
    }


    /// <summary>
    ///     Sets return type from cellular noise calculations -
    ///     Default: Distance
    /// </summary>
    /// <param name="cellularReturnType"></param>
    public void SetCellularReturnType(CellularReturnType cellularReturnType)
    {
        NativeSetCellularReturnType(nativePointer, (int)cellularReturnType);
    }

    /// <summary>
    ///     Sets distance function used in cellular noise calculations -
    ///     Default: Euclidean
    /// </summary>
    /// <param name="cellularDistanceFunction"></param>
    public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction)
    {
        NativeSetCellularDistanceFunction(nativePointer, (int)cellularDistanceFunction);
    }

    /// <summary>
    ///     Sets the type of noise used if cellular return type is set the NoiseLookup -
    ///     Default: Simplex
    /// </summary>
    /// <param name="cellularNoiseLookupType"></param>
    public void SetCellularNoiseLookupType(NoiseType cellularNoiseLookupType)
    {
        NativeSetCellularNoiseLookupType(nativePointer, (int)cellularNoiseLookupType);
    }

    /// <summary>
    ///     Sets relative frequency on the cellular noise lookup return type -
    ///     Default: 0.2
    /// </summary>
    /// <param name="cellularNoiseLookupFrequency"></param>
    public void SetCellularNoiseLookupFrequency(float cellularNoiseLookupFrequency)
    {
        NativeSetCellularNoiseLookupFrequency(nativePointer, cellularNoiseLookupFrequency);
    }

    /// <summary>
    ///     Sets the 2 distance indicies used for distance2 return types
    ///     Default: 0, 1
    ///     Note: index0 should be lower than index1
    ///     Both indicies must be >= 0, index1 must be less than 4
    /// </summary>
    /// <param name="cellularDistanceIndex0"></param>
    /// <param name="cellularDistanceIndex1"></param>
    public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
    {
        NativeSetCellularDistance2Indicies(nativePointer, cellularDistanceIndex0, cellularDistanceIndex1);
    }

    /// <summary>
    ///     Sets the maximum distance a cellular point can move from it's grid position
    ///     Setting this high will make artifacts more common
    ///     Default: 0.45
    /// </summary>
    /// <param name="cellularJitter"></param>
    public void SetCellularJitter(float cellularJitter)
    {
        NativeSetCellularJitter(nativePointer, cellularJitter);
    }


    /// <summary>
    ///     Enables position perturbing for all noise types -
    ///     Default: None
    /// </summary>
    /// <param name="perturbType"></param>
    public void SetPerturbType(PerturbType perturbType)
    {
        NativeSetPerturbType(nativePointer, (int)perturbType);
    }

    /// <summary>
    ///     Set the relative frequency for the perturb gradient -
    ///     Default: 0.5
    /// </summary>
    /// <param name="perturbFreq"></param>
    public void SetPerturbFrequency(float perturbFreq)
    {
        NativeSetPerturbFrequency(nativePointer, perturbFreq);
    }

    /// <summary>
    ///     Sets the maximum distance the input position can be perturbed -
    ///     Default: 1.0
    /// </summary>
    /// <param name="perturbAmp"></param>
    public void SetPerturbAmp(float perturbAmp)
    {
        NativeSetPerturbAmp(nativePointer, perturbAmp);
    }

    /// <summary>
    ///     Sets octave count for perturb fractal types -
    ///     Default: 3
    /// </summary>
    /// <param name="perturbOctaves"></param>
    public void SetPerturbFractalOctaves(int perturbOctaves)
    {
        NativeSetPerturbFractalOctaves(nativePointer, perturbOctaves);
    }

    /// <summary>
    ///     Sets octave lacunarity for perturb fractal types -
    ///     Default: 2.0
    /// </summary>
    /// <param name="perturbFractalLacunarity"></param>
    public void SetPerturbFractalLacunarity(float perturbFractalLacunarity)
    {
        NativeSetPerturbFractalLacunarity(nativePointer, perturbFractalLacunarity);
    }

    /// <summary>
    ///     Sets octave gain for perturb fractal types -
    ///     Default: 0.5
    /// </summary>
    /// <param name="perturbFractalGain"></param>
    public void SetPerturbFractalGain(float perturbFractalGain)
    {
        NativeSetPerturbFractalGain(nativePointer, perturbFractalGain);
    }

    /// <summary>
    ///     Sets the length for vectors after perturb normalising
    ///     Default: 1.0
    /// </summary>
    /// <param name="perturbNormaliseLength"></param>
    public void SetPerturbNormaliseLength(float perturbNormaliseLength)
    {
        NativeSetPerturbNormaliseLength(nativePointer, perturbNormaliseLength);
    }


    public void FillNoiseSet(float[] noiseSet, int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, float scaleModifier = 1.0f)
    {
        NativeFillNoiseSet(nativePointer, noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, scaleModifier);
    }

    public void FillSampledNoiseSet(float[] noiseSet, int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, int sampleScale)
    {
        NativeFillSampledNoiseSet(nativePointer, noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, sampleScale);
    }

    public void FillNoiseSetVector(float[] noiseSet, VectorSet vectorSet, float xOffset = 0f, float yOffset = 0f, float zOffset = 0f)
    {
        NativeFillNoiseSetVector(nativePointer, noiseSet, vectorSet.nativePointer, xOffset, yOffset, zOffset);
    }

    public void FillSampledNoiseSetVector(float[] noiseSet, VectorSet vectorSet, float xOffset = 0f, float yOffset = 0f, float zOffset = 0f)
    {
        NativeFillSampledNoiseSetVector(nativePointer, noiseSet, vectorSet.nativePointer, xOffset, yOffset, zOffset);
    }

    public float[] GetNoiseSet(int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, float scaleModifier = 1.0f)
    {
        var noiseSet = GetEmptyNoiseSet(xSize, ySize, zSize);
        NativeFillNoiseSet(nativePointer, noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, scaleModifier);
        return noiseSet;
    }

    public float[] GetSampledNoiseSet(int xStart, int yStart, int zStart, int xSize, int ySize, int zSize, int sampleScale)
    {
        var noiseSet = GetEmptyNoiseSet(xSize, ySize, zSize);
        NativeFillSampledNoiseSet(nativePointer, noiseSet, xStart, yStart, zStart, xSize, ySize, zSize, sampleScale);
        return noiseSet;
    }

    public float[] GetEmptyNoiseSet(int xSize, int ySize, int zSize)
    {
        return new float[xSize * ySize * zSize];
    }

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
#if UNITY_IOS && !UNITY_EDITOR
	private const string NATIVE_LIB = "__Internal";
#else
	private const string NATIVE_LIB = "FastNoiseSIMD_CLib";
#endif

	[DllImport(NATIVE_LIB)]
	public static extern int GetSIMDLevel();

	[DllImport(NATIVE_LIB)]
	public static extern void SetSIMDLevel(int level);

	[DllImport(NATIVE_LIB)]
	private static extern POINTER NewFastNoiseSIMD(int seed);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeFree(POINTER nativePointer);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetSeed(POINTER nativePointer, int seed);

	[DllImport(NATIVE_LIB)]
	private static extern int NativeGetSeed(POINTER nativePointer);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetFrequency(POINTER nativePointer, float freq);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetNoiseType(POINTER nativePointer, int noiseType);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetAxisScales(POINTER nativePointer, float xScale, float yScale, float zScale);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetFractalOctaves(POINTER nativePointer, int octaves);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetFractalLacunarity(POINTER nativePointer, float lacunarity);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetFractalGain(POINTER nativePointer, float gain);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetFractalType(POINTER nativePointer, int fractalType);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularDistanceFunction(POINTER nativePointer, int distanceFunction);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularReturnType(POINTER nativePointer, int returnType);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularNoiseLookupType(POINTER nativePointer, int noiseType);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularNoiseLookupFrequency(POINTER nativePointer, float freq);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularDistance2Indicies(POINTER nativePointer, int cellularDistanceIndex0, int cellularDistanceIndex1);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetCellularJitter(POINTER nativePointer, float cellularJitter);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbType(POINTER nativePointer, int perturbType);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbFrequency(POINTER nativePointer, float perturbFreq);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbAmp(POINTER nativePointer, float perturbAmp);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbFractalOctaves(POINTER nativePointer, int perturbOctaves);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbFractalLacunarity(POINTER nativePointer, float perturbFractalLacunarity);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbFractalGain(POINTER nativePointer, float perturbFractalGain);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeSetPerturbNormaliseLength(POINTER nativePointer, float perturbNormaliseLength);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeFillNoiseSet(POINTER nativePointer, float[] noiseSet, int xStart, int yStart, int zStart,
		int xSize, int ySize, int zSize, float scaleModifier);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeFillSampledNoiseSet(POINTER nativePointer, float[] noiseSet, int xStart, int yStart, int zStart,
		int xSize, int ySize, int zSize, int sampleScale);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeFillNoiseSetVector(POINTER nativePointer, float[] noiseSet, POINTER vectorSetPointer,
		float xOffset, float yOffset, float zOffset);

	[DllImport(NATIVE_LIB)]
	private static extern void NativeFillSampledNoiseSetVector(POINTER nativePointer, float[] noiseSet, POINTER vectorSetPointer,
		float xOffset, float yOffset, float zOffset);

#else
    public static int GetSIMDLevel()
    {
        return -2;
    }

    public static void SetSIMDLevel(int level)
    {
    }

    private static POINTER NewFastNoiseSIMD(int seed)
    {
        Debug.LogError("FastNoise SIMD not supported on this platform");
        return POINTER.Zero;
    }

    private static void NativeFree(POINTER nativePointer)
    {
    }

    private static void NativeSetSeed(POINTER nativePointer, int seed)
    {
    }

    private static int NativeGetSeed(POINTER nativePointer)
    {
        return 0;
    }

    private static void NativeSetFrequency(POINTER nativePointer, float freq)
    {
    }

    private static void NativeSetNoiseType(POINTER nativePointer, int noiseType)
    {
    }

    private static void NativeSetAxisScales(POINTER nativePointer, float xScale, float yScale, float zScale)
    {
    }

    private static void NativeSetFractalOctaves(POINTER nativePointer, int octaves)
    {
    }

    private static void NativeSetFractalLacunarity(POINTER nativePointer, float lacunarity)
    {
    }

    private static void NativeSetFractalGain(POINTER nativePointer, float gain)
    {
    }

    private static void NativeSetFractalType(POINTER nativePointer, int fractalType)
    {
    }

    private static void NativeSetCellularDistanceFunction(POINTER nativePointer, int distanceFunction)
    {
    }

    private static void NativeSetCellularReturnType(POINTER nativePointer, int returnType)
    {
    }

    private static void NativeSetCellularNoiseLookupType(POINTER nativePointer, int noiseType)
    {
    }

    private static void NativeSetCellularNoiseLookupFrequency(POINTER nativePointer, float freq)
    {
    }

    private static void NativeSetCellularDistance2Indicies(POINTER nativePointer, int cellularDistanceIndex0, int cellularDistanceIndex1)
    {
    }

    private static void NativeSetCellularJitter(POINTER nativePointer, float cellularJitter)
    {
    }

    private static void NativeSetPerturbType(POINTER nativePointer, int perturbType)
    {
    }

    private static void NativeSetPerturbFrequency(POINTER nativePointer, float perturbFreq)
    {
    }

    private static void NativeSetPerturbAmp(POINTER nativePointer, float perturbAmp)
    {
    }

    private static void NativeSetPerturbFractalOctaves(POINTER nativePointer, int perturbOctaves)
    {
    }

    private static void NativeSetPerturbFractalLacunarity(POINTER nativePointer, float perturbFractalLacunarity)
    {
    }

    private static void NativeSetPerturbFractalGain(POINTER nativePointer, float perturbFractalGain)
    {
    }

    private static void NativeSetPerturbNormaliseLength(POINTER nativePointer, float perturbNormaliseLength)
    {
    }

    private static void NativeFillNoiseSet(POINTER nativePointer, float[] noiseSet, int xStart, int yStart, int zStart,
        int xSize, int ySize, int zSize, float scaleModifier)
    {
    }

    private static void NativeFillSampledNoiseSet(POINTER nativePointer, float[] noiseSet, int xStart, int yStart, int zStart,
        int xSize, int ySize, int zSize, int sampleScale)
    {
    }

    private static void NativeFillNoiseSetVector(POINTER nativePointer, float[] noiseSet, POINTER vectorSetPointer,
        float xOffset, float yOffset, float zOffset)
    {
    }

    private static void NativeFillSampledNoiseSetVector(POINTER nativePointer, float[] noiseSet, POINTER vectorSetPointer,
        float xOffset, float yOffset, float zOffset)
    {
    }
#endif

    public class VectorSet
    {
        internal readonly POINTER nativePointer;

        public VectorSet(Vector3[] vectors, int sampleSizeX = -1, int sampleSizeY = -1, int sampleSizeZ = -1, int samplingScale = 0)
        {
            var vectorSetArray = new float[vectors.Length * 3];

            for (var i = 0; i < vectors.Length; i++)
            {
                vectorSetArray[i] = vectors[i].x;
                vectorSetArray[i + vectors.Length] = vectors[i].y;
                vectorSetArray[i + vectors.Length * 2] = vectors[i].z;
            }

            nativePointer = NewVectorSet(vectorSetArray, vectorSetArray.Length, samplingScale, sampleSizeX, sampleSizeY, sampleSizeZ);
        }

        ~VectorSet()
        {
            NativeFreeVectorSet(nativePointer);
        }

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
		[DllImport(NATIVE_LIB)]
		private static extern POINTER NewVectorSet(float[] vectorSetArray, int arraySize, int samplingScale, int sampleSizeX, int sampleSizeY, int sampleSizeZ);

		[DllImport(NATIVE_LIB)]
		private static extern void NativeFreeVectorSet(POINTER nativePointer);
#else
        private static POINTER NewVectorSet(float[] vectorSetArray, int arraySize, int samplingScale, int sampleSizeX, int sampleSizeY, int sampleSizeZ)
        {
            Debug.LogError("FastNoise SIMD Vector Set not supported on this platform");
            return POINTER.Zero;
        }

        private static void NativeFreeVectorSet(POINTER nativePointer)
        {
        }
#endif
    }
}