   M S B A M L   `   `   `  ����J  FBottleVisionApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:BottleVisionAppBottleVisionApp  5   	        BottleVisionApp.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:BottleVisionApp     5   	     � Title$  
MainWindow��5   	   $	��618��6   $	��890��6)   .��5	      � $��Center=�6   $	��849��6(   $	��562��64   .+�5
   
   �� -   5         ��Name$ videoPlayer��5
      $
��Left=�6%   $	��240��6@   $��
64,114,0,0q�6M   $	��Top=�6a   $	��320��6y   $��Uniform=�6�   �� 5   
   -   5         ��Name$ startBtn��5      $��Start��6:   $
��Left=�6J   $��50��6e   $��377,411,0,0q�6q   $	��Top=�6�   $	��130��6�   $��20(�6�   �� 5   
   -   5         ��Name$ checkBox��5      $��	Add Boxes��6%   $
��Left=�69   $��18��6T   $��377,495,0,0q�6`   $	��Top=�6u   $	��130��6�   $��16(�6�   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ datagrid��5      $
��Left=�6%   $	��122��6@   $��
51,408,0,0q�6M   $	��Top=�6a   $	��251��6y      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$  Bottle Type��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 100 6Z   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6;   �� *
bottleType+6Z     5      $  Shop��6%   $ 50 6O    64   �� *inShop+6O     5      $ 
 Warehouse��6%   $	 100 6Y    69   �� *inWarehouse+6Y   5      �� 5   
   -   5        $ stopBtn��5      $
��Stop��6"   $��50��61   $��538,411,0,0q�6=   $
��Left=�6R   $	��Top=�6m   $��20(�6�   $	��128��6�   �� 5   
   -   5        $ detectionPlayer��5      $
��Left=�6)   $	��240��6D   $��442,114,0,0q�6Q   $	��Top=�6f   $	��320��6~   $��Uniform=�6�   �� 5   
   -   5        $ status1��5      $��Status��6"   $
��Left=�63   $��699,411,0,0q�6N   $	��Top=�6c   $��50��6{   $	��128��6�   $��20(�6�   	 J� RenderTransformOrigin$	 
1.419,0.49&�6�   5      5      