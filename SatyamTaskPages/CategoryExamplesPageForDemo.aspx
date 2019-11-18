<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CategoryExamplesPageForDemo.aspx.cs" Inherits="SatyamTaskPages.CategoryExamplesPageForDemo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h3>Here are some examples for the various categories for you to make things clear.</h3>
            <table>
                <tr>
                    <td>
                        Car : 
                </td>
                    <td>

                        <asp:Image ID="Image1" runat="server" Width="100px" ImageUrl="~/Images/car1.png"/>

                    </td>
                    <td>

                        <asp:Image ID="Image2" runat="server" ImageUrl="~/Images/car2.png" Width="100px"/>

                    </td>
                    <td>

                        <asp:Image ID="Image3" runat="server" ImageUrl="~/Images/car3.png" Width="100px"/>

                    </td>
                    <td>

                        <asp:Image ID="Image4" runat="server" ImageUrl="~/Images/car4.png" Width="100px"/>

                    </td>
                </tr>
                <tr>
                    <td>
                        Bus : 
                </td>
                    <td>

                        <asp:Image ID="Image5" runat="server" ImageUrl="~/Images/bus1.png" Width="100px"/>

                    </td>
                    <td>

                        <asp:Image ID="Image6" runat="server" ImageUrl="~/Images/bus2.png" Width="100px"/>

                    </td>
                    <td>

                        <asp:Image ID="Image7" runat="server" ImageUrl="~/Images/bus3.png" Width="100px"/>

                    </td>
                    <td>

                        <asp:Image ID="Image8" runat="server" ImageUrl="~/Images/bus4.png" Width="100px"/>

                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
