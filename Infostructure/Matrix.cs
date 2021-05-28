using System;

namespace RayTracing2._0
{
    public class Matrix
    {
        private double[,] _matrix = new double[4,4]; 
        public Matrix(double[,] matrix)
        {
            _matrix = matrix;
        }

        public int RowsCount => _matrix.GetLength(1); 
        public int ColumnsCount => _matrix.GetLength(0);

        public override string ToString()
        {
            var result = "";
            for (var x = 0; x < ColumnsCount; x++)
            {
                for (int y = 0; y < RowsCount; y++)
                {
                    result += _matrix[x, y] + ", ";
                }

                result += "\n";
            }

            return result;
        }

        public Vector3 GetPoint()
        {
            return new Vector3(_matrix[0, 0], _matrix[1, 1], _matrix[2, 2]);
        }

        public static Matrix CreateStartMatrix()
        {
            return new Matrix(new[,]
            {
                {1.0, 0.0, 0.0, 0.0},
                {0.0, 1.0, 0.0, 0.0},
                {0.0, 0.0, 1.0, 0.0},
                {0.0, 0.0, 0.0, 1.0}
                
            });
        }
        
        public static Matrix CreateRotationMatrixX(double angle)
        {
            return new Matrix(new[,]
            {
                {1.0, 0.0, 0.0, 0.0},
                {0.0, Math.Cos(angle), -Math.Sin(angle), 0.0},
                {0.0, Math.Sin(angle), Math.Cos(angle), 0.0},
                {0.0, 0.0, 0.0, 1.0}
            });
        }
        
        public static Matrix CreateRotationMatrixY(double angle)
        {
            return new Matrix(new[,]
            {
                {Math.Cos(angle),  0.0,             Math.Sin(angle),0.0},
                {0.0,              1.0,             0.0,             0.0},
                {-Math.Sin(angle),  0.0,             Math.Cos(angle), 0.0},
                {0.0,              0.0,             0.0,             1.0}
                
            });
        }
        
        public static Matrix CreateRotationMatrixZ(double angle)
        {
            return new Matrix(new[,]
            {
                {Math.Cos(angle), -Math.Sin(angle), 0.0, 0.0},
                {Math.Sin(angle),  Math.Cos(angle), 0.0, 0.0},
                {0.0,              0.0,             1.0, 0.0},         
                {0.0,              0.0,             0.0, 1.0}
                
            });
        }
        
        public static Matrix CreateTransformMatrixFromView(double len)
        {
            return new Matrix(new[,]
            {
                {1.0, 0.0, 0.0, 0.0},
                {0.0, 1.0, 0.0, 0.0},
                {0.0, 0.0, 0.0, 0.0},
                {0.0, 0.0, 1 / len , 1.0}});
        }
        
        public static Matrix CreateScaleMatrix(double scale)
        {
            return new Matrix(new[,]
            {
                {scale, 0.0,   0.0,   0.0},
                {0.0,   scale, 0.0,   0.0},
                {0.0,   0.0,   scale, 0.0},         
                {0.0,   0.0,   0.0,   scale}
            });
        }
        
        public static Matrix CreateTranslateMatrix(double dx, double dy, double dz)
        {
            return new Matrix(new[,]
            {
                {1.0, 0.0, 0.0, dx },
                {0.0, 1.0, 0.0, dy },
                {0.0, 0.0, 1.0, dz },
                {0.0, 0.0, 0.0, 1.0}
            });
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            var result = MatrixMultiplication(a._matrix, b._matrix);
            return new Matrix(result);
        }

        public static Vector3 operator *(Matrix a, Vector3 b)
        {
            var result = MatrixMultiplication(a._matrix, b.ToMatrix()._matrix);

            return new Vector3(result[0, 0], result[1, 0], result[2, 0]);
        }
        
        public static Vector3 operator *(Vector3 a, Matrix b)
        {
            return b * a;
        }
        
        
        private static double[,] MatrixMultiplication(double[,] a, double[,] b)
        {
            if (a.GetLength(1) != b.GetLength(0))
                throw new Exception("Матрицы нельзя перемножить");
            
            var r = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    for (int k = 0; k < b.GetLength(0); k++)
                    {
                        r[i,j] += a[i,k] * b[k,j];
                    }
                }
            }

            return r;
        }

        protected double this[int x, int y] => _matrix[x, y];
    }
}