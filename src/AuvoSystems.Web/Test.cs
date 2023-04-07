namespace AuvoSystems.Web
{
    using System;

    namespace PontoFuncionarios
    {
        public class Test
        {
            public void Main()
            {
                string nomeFuncionario = "João Silva";
                int diasTrabalhados = 20;
                int horasTrabalhadasPorDia = 8;
                double valorHora = 20.0;
                int diasFaltantes = 0;
                int horasFaltantes = 0;
                int horasExtras = 0;
                int diasExtras = 0;

                double salario = calcularSalario(valorHora, horasTrabalhadasPorDia, diasTrabalhados, diasFaltantes, horasFaltantes, horasExtras, diasExtras);
                double valorDescontado = calcularDescontos(valorHora, horasTrabalhadasPorDia, diasTrabalhados, diasFaltantes, horasFaltantes);

                Console.WriteLine("Nome do funcionário: " + nomeFuncionario);
                Console.WriteLine("Salário: R$ " + salario);
                Console.WriteLine("Descontos: R$ " + valorDescontado);
            }

            static double calcularSalario(double valorHora, int horasTrabalhadasPorDia, int diasTrabalhados, int diasFaltantes, int horasFaltantes, int horasExtras, int diasExtras)
            {
                double salario = 0;
                salario = (valorHora * horasTrabalhadasPorDia * diasTrabalhados) + (valorHora * horasExtras * 1.5) + (valorHora * horasFaltantes * -1) + (valorHora * horasExtras * 1.5);
                salario += (valorHora * horasTrabalhadasPorDia * diasExtras);
                return salario;
            }

            static double calcularDescontos(double valorHora, int horasTrabalhadasPorDia, int diasTrabalhados, int diasFaltantes, int horasFaltantes)
            {
                double valorDescontado = 0;
                valorDescontado = (valorHora * horasTrabalhadasPorDia * diasFaltantes) + (valorHora * horasFaltantes * -1);
                return valorDescontado;
            }
        }
    }

}
