   M S B A M L   `   `   `  ����E  AMVVM_test1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:MVVM_test1.ViewModelsMVVM_test1.ViewModels  5   	   =#clr-namespace:MVVM_test1.ViewModelsMVVM_test1.ViewModels  5   	   %    MVVM_test1.Views.MainWindowView   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35a ]DevExpress.Xpf.Core.v16.1, Version=16.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   2vm#clr-namespace:MVVM_test1.ViewModels     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:MVVM_test1.ViewModels     5   	     � Title$  MainWindowView��5	   	   $	��350��6    $	��525��6-    � DataContext 5
      %  MVVM_test1.ViewModels.ViewModel  5   
   .��5      � .+�5   
   �� -   5         ��Name$ textBox��5   L   $
��Left=�6]   $��23��6x   $��
196,71,0,0q�6�    �� TextWrapping$
 Wrap=�6�   $	��Top=�6�   $	��120��6�   ��6   �� *text+ �� UpdateSourceTrigger$ PropertyChanged=�6�   �� 5   
   -   5        $ textBox_copied_text��5   (   $
��Left=�6E   $��23��6`   $��317,163,0,0q�6l   $
 Wrap=�6�   $	��Top=�6�   $	��120��6�   ��6   �� *text+6�   �� 5   
   -   5        $ textBox_converted_text��5   2   $
��Left=�6R   $��23��6m   $��
77,163,0,0q�6y   $
 Wrap=�6�   $	��Top=�6�   $	��120��6�   ��6   �� *converted_text+6�   �� 5   
   -   5         ��Name$ button��5   4   $��Convert��6D   $
��Left=�6V   $��25��6q   $��221,122,0,0q�6}   $	��Top=�6�   $��70��6�   ��6   �� *ConvertCommand+6�   �� 5   
   -   5         ��Name$ 	textBlock��5      $
��Left=�6'   $��
105,75,0,0q�6B   $
��Wrap=�6V   $��Write text here:��6j   $	��Top=�6�   �� 5   
   -   5         ��Name$ listBox��5   5   $
��Left=�6F   $	��110��6a   $��
77,195,0,0q�6n   $	��Top=�6�   $	��120��6�   ��6   �� *
returnList+6�   5      5      