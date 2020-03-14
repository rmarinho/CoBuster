using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms;

namespace CoBuster
{
	public class VitalSignsProcessing
	{
		int processing = 0;

		//Freq + timer variable
		static long startTime = 0;
		double SamplingFreq;

		// SPO2 variables
		static double RedBlueRatio = 0;
		double Stdr = 0;
		double Stdb = 0;
		double sumred = 0;
		double sumblue = 0;
		public int o2;

		//Arraylist
		public List<Double> RedAvgList = new List<Double>();
		public List<Double> BlueAvgList = new List<Double>();
		public int counter = 0;

		public int O2Processsing(byte[] data, Size size)
		{
			if (Interlocked.Exchange(ref processing, 1) == 1)
			{
				return -1;
			}

			int width = (int)size.Width;
			int height = (int)size.Height;
			double RedAvg;
			double BlueAvg;

			RedAvg = ImageProcessing.DecodeYUV420SPtoRedBlueGreenAvg(data, height, width, 1); //1 stands for red intensity, 2 for blue, 3 for green
			sumred = sumred + RedAvg;
			BlueAvg = ImageProcessing.DecodeYUV420SPtoRedBlueGreenAvg(data, height, width, 2); //1 stands for red intensity, 2 for blue, 3 for green
			sumblue = sumblue + BlueAvg;

			RedAvgList.Add(RedAvg);
			BlueAvgList.Add(BlueAvg);

			++counter; //countes number of frames in 30 seconds

			//To check if we got a good red intensity to process if not return to the condition and set it again until we get a good red intensity
			if (RedAvg < 200)
			{
				//inc = 0;
				//ProgP = inc;
				//ProgO2.setProgress(ProgP);
				Interlocked.Exchange(ref processing, 0);
			}

			long endTime = DateTime.Now.Millisecond;
			double totalTimeInSecs = (endTime - startTime) / 1000d; //to convert time to seconds

			if (totalTimeInSecs >= 30)
			{ //when 30 seconds of measuring passes do the following " we chose 30 seconds to take half sample since 60 seconds is normally a full sample of the heart beat

				startTime = DateTime.Now.Millisecond;
				SamplingFreq = (counter / totalTimeInSecs);
				Double[] Red = RedAvgList.ToArray();
				Double[] Blue = BlueAvgList.ToArray();
				//MathNet.Numerics.IntegralTransforms.Fourier.FrequencyScale(Red, counter, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
				double HRFreq = 1.0; //;  Fft.FFT(Red, counter, SamplingFreq);
				double bpm = (int)Math.Ceiling(HRFreq * 60);

				double meanr = sumred / counter;
				double meanb = sumblue / counter;

				for (int i = 0; i < counter - 1; i++)
				{

					Double bufferb = Blue[i];

					Stdb = Stdb + ((bufferb - meanb) * (bufferb - meanb));

					Double bufferr = Red[i];

					Stdr = Stdr + ((bufferr - meanr) * (bufferr - meanr));

				}

				double varr = Math.Sqrt(Stdr / (counter - 1));
				double varb = Math.Sqrt(Stdb / (counter - 1));

				double R = (varr / meanr) / (varb / meanb);

				double spo2 = 100 - 5 * (R);
				o2 = (int)(spo2);

				if ((o2 < 80 || o2 > 99) || (bpm < 45 || bpm > 200))
				{
					//inc = 0;
					//ProgP = inc;
					//ProgO2.setProgress(ProgP);
					//mainToast = Toast.makeText(getApplicationContext(), "Measurement Failed", Toast.LENGTH_SHORT);
					//mainToast.show();
					//startTime = System.currentTimeMillis();
					//counter = 0;
					Interlocked.Exchange(ref processing, 0);
					return -2;
				}

			}

			if (o2 != 0)
			{
				

			}

			if (RedAvg != 0)
			{
				//                    ProgP=inc++/34;;
				//                    ProgO2.setProgress(ProgP);
			}

			Interlocked.Exchange(ref processing, 0);

			return o2;
		}
	}
}
