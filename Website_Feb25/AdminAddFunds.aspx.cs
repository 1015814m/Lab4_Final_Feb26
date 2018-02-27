
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using database;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

public partial class AdminAddFunds : System.Web.UI.Page
{
    Employee user;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["employeeLoggedIn"] == null)
        {
            Response.Redirect("Login.aspx");
        }
        if (Session["employeeLoggedIn"].ToString() != "True")
        {
            Response.Redirect("Login.aspx");
        }

        user = (Employee)Session["user"];

        if (user.Admin != true)
        {
            Response.Redirect("HomePage.aspx");
        }

        checkFunds();

        lblCurrenFundsNum.Text = " $" + Decimal.Round(currentFunds(), 2);
        lblTotalPoints.Text = " $" + Decimal.Round(totalEarned(), 2);
        Decimal remaining = currentFunds() - totalEarned();
        lblRemainingFunds.Text = " $" + Decimal.Round(remaining, 2);
    }

    protected void SubmitFunds_OnClick(object sender, EventArgs e)
    {
        try
        {
            Boolean check = true;
            //validation that all entries are filled in
            if (txtAmount.Text == "")
            {
                check = false;
                lblError.Visible = true;
                lblError.Text = "*Please enter an amount.";
                txtAmount.Focus();
            }
            if (txtAmount.Text == "0")
            {
                check = false;
                lblError.Visible = true;
                lblError.Text = "*Please enter an amount greater than 0.";
                txtAmount.Focus();
            }
            if (txtDepositTo.Text == "")
            {
                check = false;
                lblError.Visible = true;
                lblError.Text = "*Please enter an account number to deposit to.";
                txtDepositTo.Focus();
            }
            if (txtWithdrawFrom.Text == "")
            {
                check = false;
                lblError.Visible = true;
                lblError.Text = "*Please enter an account number to withdraw from.";
                txtWithdrawFrom.Focus();
            }
            if (check)
            {
                sendFunds();
                lblError.Visible = true;
                lblError.Text = "Funds transferred successfully!";
            }
        }
        catch (Exception)
        {
            lblError.Text = "There was a problem transferring funds.";
        }
    }

    public void sendFunds()
    {
        try
        {
            string commandText = "INSERT INTO [dbo].[Fund] (AccountTo, AccountFrom, Amount) Values (@AccountTo, @AccountFrom, @Amount)";
            SqlConnection conn = ProjectDB.connectToDB();
            SqlCommand insert = new SqlCommand(commandText, conn);
            insert.Parameters.AddWithValue("@AccountTo", txtDepositTo.Text);
            insert.Parameters.AddWithValue("@AccountFrom", txtWithdrawFrom.Text);
            insert.Parameters.AddWithValue("@Amount", txtAmount.Text);
            insert.ExecuteNonQuery();
            conn.Close();
        }
        catch (Exception)
        {
            lblError.Text = "There was a problem transferring funds.";
        }
    }
    
    protected Decimal totalEarned()
    {
        Decimal funds = 0;
        try
        {
            string commandText = "SELECT SUM([Points]) as Result FROM [dbo].[Employee]";
            SqlConnection conn = ProjectDB.connectToDB();
            SqlCommand select = new SqlCommand(commandText, conn);

            SqlDataReader reader = select.ExecuteReader();

            if(reader.HasRows)
            {
                reader.Read();
                funds = (Decimal)reader["Result"];
            }
            conn.Close();
        }
        catch (Exception)
        {

        }
        return funds;
    }

    protected Decimal currentFunds()
    {
        Decimal earned = 0;
        try
        {
            string commandText = "SELECT SUM([Amount]) as Result FROM [dbo].[Fund]";
            SqlConnection conn = ProjectDB.connectToDB();
            SqlCommand select = new SqlCommand(commandText, conn);

            SqlDataReader reader = select.ExecuteReader();

            if(reader.HasRows)
            {
                reader.Read();
                earned = (Decimal)reader["Result"];
            }
            conn.Close();
        }
        catch (Exception)
        {

        }
        return earned;
    }

    protected void checkFunds()
    {
        try
        {
            if ((currentFunds() - 500) <= totalEarned() || currentFunds() <= 500)
            {
                Email fund = new Email(user.Email, "The funds in your reward account are getting low!", "Low Fund Alert");
                fund.sendEmail();
            }

        }
        catch (Exception)
        {

        }
    }



}